; This program and the accompanying materials are made available under the
; terms of the MIT license (X11 license) which accompanies this distribution.

; Author: C. Bürger

#!r6rs

(library
 (tinycpp-racr normalization)
 (export
  transform-to-normalform)
 (import (rnrs) (racr core) (tinycpp-racr exception-api))
 
 (define transform-to-normalform
   (lambda (compilation-unit)
     (ast-for-each-child
      (lambda (i n)
        (unless (att-value 'qualified-declaration? n)
          (add-default-constructors n)))
      (ast-child 'Body compilation-unit))
     (weave-inner-classes compilation-unit)))
 
 (define add-default-constructors
   (lambda (n)
     (when (ast-subtype? n 'ClassDefinition)
       (rewrite-add
        (ast-child 'Body n)
        (create-ast
         (ast-specification n)
         'Constructor
         (list
          (ast-child 'name n)
          (create-ast-list (list))
          (create-ast-list (list)))))
       (ast-for-each-child
        (lambda (i n)
          (add-default-constructors n))
        (ast-child 'Body n)))))
 
 (define weave-inner-classes
   (lambda (compilation-unit)
     (let* ((well-formed?
             (att-value 'correct? compilation-unit))
            (source-definition?
             (and well-formed? (att-value 'next-qualified-declaration? compilation-unit)))
            (target-declaration?
             (and source-definition? (att-value 'lookup-reference source-definition? (ast-child 'name source-definition?)))))
       (when source-definition?
         (unless (and target-declaration? (eq? (ast-node-type target-declaration?) 'ClassDeclaration))
           (throw-tinycpp-racr-exception "ERROR: Program not well-formed."))
         (rewrite-refine
          target-declaration?
          'ClassDefinition
          (rewrite-subtree
           (ast-child 'Body source-definition?)
           (create-ast-list (list))))
         (add-default-constructors target-declaration?)
         (rewrite-delete source-definition?)
         (weave-inner-classes compilation-unit))))))