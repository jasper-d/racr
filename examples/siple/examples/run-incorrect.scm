; This program and the accompanying materials are made available under the
; terms of the MIT license (X11 license) which accompanies this distribution.

; Author: C. Bürger

#!r6rs

(import (rnrs) (siple main) (siple exception-api) (racr testing))

(assert-exception
 siple-exception?
 (siple-interpret (cadr (command-line))))