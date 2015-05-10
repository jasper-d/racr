; This program and the accompanying materials are made available under the
; terms of the MIT license (X11 license) which accompanies this distribution.

; Author: C. Bürger

#!r6rs

(library
 (ttc-2015-fuml-activity-diagrams language)
 (export exception: Boolean Integer Undefined && //
         :Activity :Variable :ActivityEdge :ControlFlow :InitialNode :FinalNode :ForkNode
         :JoinNode :DecisionNode :MergeNode :ExecutableNode :UnaryExpression :BinaryExpression
         ->name ->initial ->source ->target
         =variables =edges =v-lookup =e-lookup =valid? =petrinet)
 (import (rnrs) (racr core) (prefix (atomic-petrinets analyses) pn:))
 
 (define spec                 (create-specification))
 
 ; AST Accessors:
 (define (->name n)           (ast-child 'name n))
 (define (->type n)           (ast-child 'type n))
 (define (->initial n)        (ast-child 'initial n))
 (define (->source n)         (ast-child 'source n))
 (define (->target n)         (ast-child 'target n))
 (define (->guard n)          (ast-child 'guard n))
 (define (->assignee n)       (ast-child 'assignee n))
 (define (->operator n)       (ast-child 'operator n))
 (define (->operand1 n)       (ast-child 'operand1 n))
 (define (->operand2 n)       (ast-child 'operand2 n))
 (define (<- n)               (ast-parent n))
 
 ; Attribute Accessors:
 (define (=variables n)       (att-value 'variables n))
 (define (=nodes n)           (att-value 'nodes n))
 (define (=edges n)           (att-value 'edges n))
 (define (=expressions n)     (att-value 'expressions n))
 (define (=v-lookup n name)   (hashtable-ref (att-value 'v-lookup n) name #f))
 (define (=n-lookup n name)   (hashtable-ref (att-value 'n-lookup n) name #f))
 (define (=e-lookup n name)   (hashtable-ref (att-value 'e-lookup n) name #f))
 (define (=source n)          (att-value 'source n))
 (define (=target n)          (att-value 'target n))
 (define (=outgoing n)        (att-value 'outgoing n))
 (define (=incoming n)        (att-value 'incoming n))
 (define (=initial n)         (att-value 'initial n))
 (define (=final n)           (att-value 'final n))
 (define (=well-typed? n)     (att-value 'well-typed? n))
 (define (=valid? n)          (att-value 'valid? n))
 (define (=petrinet n)        (att-value 'petrinet n))
 (define (=places n)          (att-value 'places n))
 (define (=transitions n)     (att-value 'transitions n))
 (define (=computation n)     (att-value 'computation n))
 
 ; Type Support:
 (define (Boolean)            #f)
 (define (Integer)            #f)
 (define (Undefined)          #f)
 
 ; Operator support:
 (define (&& . a)             (for-all (lambda (x) x) a))
 (define (// . a)             (find (lambda (x) x) a))
 
 ; AST Constructors:
 (define (:Activity id v n e)
   (create-ast spec 'Activity (list id (create-ast-list v) (create-ast-list n) (create-ast-list e))))
 (define (:Variable id t i)
   (create-ast spec 'Variable (list id t i)))
 (define (:ActivityEdge id s t)
   (create-ast spec 'ActivityEdge (list id s t)))
 (define (:ControlFlow id s t g)
   (create-ast spec 'ControlFlow (list id s t g)))
 (define (:InitialNode id)
   (create-ast spec 'InitialNode (list id)))
 (define (:FinalNode id)
   (create-ast spec 'FinalNode (list id)))
 (define (:ForkNode id)
   (create-ast spec 'ForkNode (list id)))
 (define (:JoinNode id)
   (create-ast spec 'JoinNode (list id)))
 (define (:DecisionNode id)
   (create-ast spec 'DecisionNode (list id)))
 (define (:MergeNode id)
   (create-ast spec 'MergeNode (list id)))
 (define (:ExecutableNode id e)
   (create-ast spec 'ExecutableNode (list id (create-ast-list e))))
 (define (:UnaryExpression a op op1)
   (create-ast spec 'UnaryExpression (list a op op1)))
 (define (:BinaryExpression a op op1 op2)
   (create-ast spec 'BinaryExpression (list a op op1 op2)))
 
 ;;; Exceptions:
 
 (define-condition-type fuml-exception &violation make-fuml-exception fuml-exception?)
 (define (exception: message)
   (raise-continuable (condition (make-fuml-exception) (make-message-condition message))))
 
 ;;; AST Scheme:
 
 (with-specification
  spec
  (ast-rule 'Activity->name-Variable*-ActivityNode*-ActivityEdge*)
  (ast-rule 'Variable->name-type-initial)
  (ast-rule 'ActivityEdge->name-source-target)
  (ast-rule 'ControlFlow:ActivityEdge->guard)
  (ast-rule 'ActivityNode->name)
  (ast-rule 'InitialNode:ActivityNode->)
  (ast-rule 'FinalNode:ActivityNode->)
  (ast-rule 'ForkNode:ActivityNode->)
  (ast-rule 'JoinNode:ActivityNode->)
  (ast-rule 'DecisionNode:ActivityNode->)
  (ast-rule 'MergeNode:ActivityNode->)
  (ast-rule 'ExecutableNode:ActivityNode->Expression*)
  (ast-rule 'Expression->assignee-operator)
  (ast-rule 'UnaryExpression:Expression->operand1)
  (ast-rule 'BinaryExpression:Expression->operand1-operand2)
  (compile-ast-specifications 'Activity))
 
 ;;; Query Support:
 
 (with-specification
  spec
  (ag-rule variables   (Activity       (lambda (n) (ast-children (ast-child 'Variable* n)))))
  (ag-rule nodes       (Activity       (lambda (n) (ast-children (ast-child 'ActivityNode* n)))))
  (ag-rule edges       (Activity       (lambda (n) (ast-children (ast-child 'ActivityEdge* n)))))
  (ag-rule expressions (ExecutableNode (lambda (n) (ast-children (ast-child 'Expression* n))))))
 
 ;;; Name Analysis:
 
 (with-specification
  spec
  
  (define (make-symbol-table -> l)
    (define table (make-eq-hashtable))
    (for-each (lambda (n) (hashtable-set! table (-> n) n)) l)
    table)
  
  (define (make-connection-table -> l)
    (define table (make-eq-hashtable))
    (for-each (lambda (n) (hashtable-update! table (-> n) (lambda (v) (cons n v)) (list))) l)
    table)
  
  (ag-rule v-lookup (Activity     (lambda (n) (make-symbol-table ->name (=variables n)))))
  (ag-rule n-lookup (Activity     (lambda (n) (make-symbol-table ->name (=nodes n)))))
  (ag-rule e-lookup (Activity     (lambda (n) (make-symbol-table ->name (=edges n)))))
  (ag-rule source   (ActivityEdge (lambda (n) (=n-lookup n (->source n)))))
  (ag-rule target   (ActivityEdge (lambda (n) (=n-lookup n (->target n)))))
  
  (ag-rule
   outgoing
   (Activity     (lambda (n) (make-connection-table ->source (=edges n))))
   (ActivityNode (lambda (n) (hashtable-ref (=outgoing (<- n)) (->name n) (list)))))
  
  (ag-rule
   incoming
   (Activity     (lambda (n) (make-connection-table ->target (=edges n))))
   (ActivityNode (lambda (n) (hashtable-ref (=incoming (<- n)) (->name n) (list)))))
  
  (ag-rule
   initial
   (Activity     (lambda (n) (find (lambda (n) (ast-subtype? n 'InitialNode)) (=nodes n)))))
  
  (ag-rule
   final
   (Activity     (lambda (n) (find (lambda (n) (ast-subtype? n 'FinalNode)) (=nodes n))))))
 
 ;;; Type Analysis:
 
 (with-specification
  spec
  
  (ag-rule
   well-typed?
   
   (Variable
    (lambda (n)
      (if (eq? (->type n) Boolean) (boolean? (->initial n)) (integer? (->initial n)))))
   
   (UnaryExpression
    (lambda (n)
      (define ass (=v-lookup n (->assignee n)))
      (define op (=v-lookup n (->operand1 n)))
      (and ass op (eq? (->type op) Boolean) (eq? (->type ass) Boolean))))
   
   (BinaryExpression
    (lambda (n)
      (define ass (=v-lookup n (->assignee n)))
      (define op1 (=v-lookup n (->operand1 n)))
      (define op2 (=v-lookup n (->operand2 n)))
      (define (in . l) (memq (->operator n) l))
      (define (op-type t) (and (eq? (->type op1) t) (eq? (->type op2) t)))
      (and ass op1 op2
           (or (and (in + -) (op-type Integer) (eq? (->type ass) Integer))
               (and (in < <= = > >=) (op-type Integer) (eq? (->type ass) Boolean))
               (and (in && //) (op-type Boolean) (eq? (->type ass) Boolean))))))))
 
 ;;; Well-formedness:
 
 (with-specification
  spec
  
  (define (in n f s)          (f (length (=incoming n)) s))
  (define (out n f s)         (f (length (=outgoing n)) s))
  (define (guarded n g)
    (define (guarded n)
      (if (ast-subtype? n 'ControlFlow)
          (let ((var (=v-lookup n (->guard n))))
            (and g var (eq? (->type var) Boolean)))
          (not g)))
    (for-all guarded (=outgoing n)))
  
  (ag-rule
   valid?
   (Variable       (lambda (n) (=well-typed? n)))
   (ActivityEdge   (lambda (n) (eq? (=e-lookup n (->name n)) n) (=source n) (=target n)))
   (ControlFlow    (lambda (n)
                     (and (eq? (=e-lookup n (->name n)) n) (=source n) (=target n)
                          (let ((v (=v-lookup n (->guard n)))) (and v (eq? (->type v) Boolean))))))
   (InitialNode    (lambda (n) (and (in n = 0) (out n = 1) (guarded n #f) (eq? (=initial n) n))))
   (FinalNode      (lambda (n) (and (in n = 1) (out n = 0) (guarded n #f) (eq? (=final n) n))))
   (ForkNode       (lambda (n) (and (in n = 1) (out n > 1) (guarded n #f))))
   (JoinNode       (lambda (n) (and (in n > 1) (out n = 1) (guarded n #f))))
   (DecisionNode   (lambda (n) (and (in n = 1) (out n >= 1) (guarded n #t))))
   (MergeNode      (lambda (n) (and (in n >= 1) (out n = 1) (guarded n #f))))
   (ExecutableNode (lambda (n) (and (in n = 1) (out n = 1) (guarded n #f)
                                    (for-all =well-typed? (=expressions n)))))
   (Activity
    (lambda (n)
      (and (=initial n) (=final n)
           (for-all =valid? (=variables n))
           (for-all =valid? (=nodes n))
           (for-all =valid? (=edges n)))))))
 
 ;;; Code Generation:
 
 (with-specification
  spec
  
  (define (v-token n ->)
    (ast-child 1 (pn:->Token* (=places (=v-lookup n (-> n))))))
  (define (v-value n ->)
    (let ((n (v-token n ->))) (lambda x (pn:->value n))))
  (define (>>? n)
    (if (ast-subtype? n 'ControlFlow)
        (pn::Arc (->source n) (list (v-value n ->guard)))
        (pn::Arc (->source n) (list (lambda (t) #t)))))
  (define (n>> n)
    (pn::Arc (->target n) (=computation (=target n))))
  (define (v-name n)
    (string->symbol (string-append "variable@" (symbol->string n))))
  
  (ag-rule
   petrinet
   (Activity       (lambda (n) (pn::AtomicPetrinet (=places n) (=transitions n)))))
  
  (ag-rule
   places
   (Activity       (lambda (n) (append (map =places (=variables n)) (map =places (=nodes n)))))
   (Variable       (lambda (n) (pn::Place (v-name (->name n)) (pn::Token (->initial n)))))
   (ActivityNode   (lambda (n) (pn::Place (->name n))))
   (InitialNode    (lambda (n) (pn::Place (->name n) (pn::Token #t)))))
  
  (ag-rule
   transitions
   
   (Activity
    (lambda (n)
      (fold-left (lambda (result n) (append (=transitions n) result)) (list) (=nodes n))))
   
   ; Construction constraints:
   ;  (2) Nodes construct their predecessor transitions, except for fork predecessors
   ;  (2) Nodes, except forks, do not construct their successor transitions
   
   (ActivityNode
    (lambda (n)
      (fold-left
       (lambda (transitions incoming)
         (if (ast-subtype? (=source incoming) 'ForkNode)
             transitions
             (cons
              (pn::Transition
               (->name incoming)
               (list (>>? incoming))
               (list (n>> incoming)))
              transitions)))
       (list)
       (=incoming n))))
   
   (ForkNode
    (lambda (n)
      (define incoming (car (=incoming n)))
      (define outgoing (=outgoing n))
      (list
       (pn::Transition
        (->name incoming)
        (list (>>? incoming))
        (list (n>> incoming)))
       (pn::Transition
        (->name (car outgoing))
        (list (>>? (car outgoing)))
        (map n>> outgoing)))))
   
   (JoinNode
    (lambda (n)
      (define incoming (=incoming n))
      (list
       (pn::Transition
        (->name (car incoming))
        (map >>? incoming)
        (list (n>> (car incoming))))))))
  
  (ag-rule
   computation
   
   (ActivityNode
    (lambda (n)
      (define trace (->name n))
      (lambda x (display trace) #t)))
   
   (ExecutableNode
    (lambda (n)
      (define trace (->name n))
      (define computations (map =computation (=expressions n)))
      (lambda x (display trace) (for-each apply computations) #t)))
   
   (UnaryExpression
    (lambda (n)
      (define assignee (v-token n ->assignee))
      (define op1 (v-value n ->operand1))
      (define op (->operator n))
      (lambda () (rewrite-terminal assignee 'value (op (op1))))))
   
   (BinaryExpression
    (lambda (n)
      (define assignee (v-token n ->assignee))
      (define op1 (v-value n ->operand1))
      (define op2 (v-value n ->operand2))
      (define op (->operator n))
      (lambda () (rewrite-terminal assignee 'value (op (op1) (op2))))))))
 
 (compile-ag-specifications spec))