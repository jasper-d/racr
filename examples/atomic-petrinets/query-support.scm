; This program and the accompanying materials are made available under the
; terms of the MIT license (X11 license) which accompanies this distribution.

; Author: C. Bürger

#!r6rs

(library
 (atomic-petrinets query-support)
 (export specify-query-support pn petrinets-exception? exception:
         :AtomicPetrinet :Place :Token :Transition :Arc
         ->Place* ->Transition* ->Token* ->In ->Out ->name ->value ->place ->consumers ->* <-
         =p-lookup =t-lookup =in-lookup =out-lookup =place =valid? =enabled? =executor
         =places =transitions =in-arcs =out-arcs)
 (import (rnrs) (racr core))
 
 (define pn                   (create-specification))
 
 ; Attribute Accessors:
 (define (=p-lookup n name)   (hashtable-ref (att-value 'p-lookup n) name #f))
 (define (=t-lookup n name)   (hashtable-ref (att-value 't-lookup n) name #f))
 (define (=in-lookup n name)  (hashtable-ref (att-value 'in-lookup n) name #f))
 (define (=out-lookup n name) (hashtable-ref (att-value 'out-lookup n) name #f))
 (define (=place n)           (att-value 'place n))
 (define (=valid? n)          (att-value 'valid? n))
 (define (=enabled? n)        (att-value 'enabled? n))
 (define (=executor n)        (att-value 'executor n))
 (define (=places n)          (att-value 'places n))
 (define (=transitions n)     (att-value 'transitions n))
 (define (=in-arcs n)         (att-value 'in-arcs n))
 (define (=out-arcs n)        (att-value 'out-arcs n))
 
 ; AST Accessors:
 (define (->Place* n)         (ast-child 'Place* n))
 (define (->Transition* n)    (ast-child 'Transition* n))
 (define (->Token* n)         (ast-child 'Token* n))
 (define (->In n)             (ast-child 'In n))
 (define (->Out n)            (ast-child 'Out n))
 (define (->name n)           (ast-child 'name n))
 (define (->value n)          (ast-child 'value n))
 (define (->place n)          (ast-child 'place n))
 (define (->consumers n)      (ast-child 'consumers n))
 (define (->* n)              (ast-children n))
 (define (<- n)               (ast-parent n))
 
 ; AST Constructors:
 (define (:AtomicPetrinet p t)
   (create-ast pn 'AtomicPetrinet (list (create-ast-list p) (create-ast-list t))))
 (define (:Place n . t)
   (create-ast pn 'Place (list n (create-ast-list t))))
 (define (:Token v)
   (create-ast pn 'Token (list v)))
 (define (:Transition n i o)
   (create-ast pn 'Transition (list n (create-ast-list i) (create-ast-list o))))
 (define (:Arc p f)
   (create-ast pn 'Arc (list p f)))
 
 ;;; Exceptions:
 
 (define-condition-type petrinets-exception
   &violation
   make-petrinets-exception
   petrinets-exception?)
 
 (define (exception: message)
   (raise-continuable
    (condition
     (make-petrinets-exception)
     (make-message-condition message))))
 
 (define (specify-query-support)
   (with-specification
    pn
    (ag-rule places      (AtomicPetrinet (lambda (n) (->* (->Place* n)))))
    (ag-rule transitions (AtomicPetrinet (lambda (n) (->* (->Transition* n)))))
    (ag-rule in-arcs     (Transition     (lambda (n) (->* (->In n)))))
    (ag-rule out-arcs    (Transition     (lambda (n) (->* (->Out n))))))))