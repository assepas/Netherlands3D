/*
 * Copyright 2020, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
mergeInto(LibraryManager.library, {
    initWebGLCopyAndPaste__postset: '_initWebGLCopyAndPaste();',
    initWebGLCopyAndPaste: function () {
        // for some reason only on Safari does Unity call
        // preventDefault so let's prevent preventDefault
        // so the browser will generate copy and paste events
		window.preventNativeCopyEvents = true;
		
        window.addEventListener = function (origFn) {
            function noop() {}

            // I hope c,x,v are universal
            const keys = {
                'c': true,
                'x': true,
                'v': true
            };

            // Emscripten doesn't support the spread operator or at
            // least the one used by Unity 2019.4.1
            return function (name, fn) {
                const args = Array.prototype.slice.call(arguments);
                if (name !== 'keypress') {
                    return origFn.apply(this, args);
                }
                args[1] = function (event) {
                    const hArgs = Array.prototype.slice.call(arguments);
                    if (keys[event.key.toLowerCase()] &&
                        ((event.metaKey ? 1 : 0) + (event.ctrlKey ? 1 : 0)) === 1) {
                        event.preventDefault = noop;
                    }
                    return fn.apply(this, hArgs);
                };
                return origFn.apply(this, args);
            };
        }
        (window.addEventListener);
        _initWebGLCopyAndPaste = function (objectNamePtr, cutCopyFuncNamePtr, pasteFuncNamePtr) {
            window.becauseUnityIsBadWithJavascript_webglCopyAndPaste =
                window.becauseUnityIsBadWithJavascript_webglCopyAndPaste || {
                initialized: false,
                objectName: UTF8ToString(objectNamePtr),
                cutCopyFuncName: UTF8ToString(cutCopyFuncNamePtr),
                pasteFuncName: UTF8ToString(pasteFuncNamePtr),
            };
            const g = window.becauseUnityIsBadWithJavascript_webglCopyAndPaste;

            if (!g.initialized) {
                window.addEventListener('cut', function (e) {
                    e.preventDefault();
                    SendMessage(g.objectName, g.cutCopyFuncName, 'x');
                    event.clipboardData.setData('text/plain', g.clipboardStr);
                });
                window.addEventListener('copy', function (e) {
                    //We can optionaly disable this so we can do a copy from a button
                    if (window.preventNativeCopyEvents == true) {
                        e.preventDefault();
                    }
                    window.preventNativeCopyEvents = true;
                    SendMessage(g.objectName, g.cutCopyFuncName, 'c');
                    event.clipboardData.setData('text/plain', g.clipboardStr);
                });
                window.addEventListener('paste', function (e) {
                    const str = e.clipboardData.getData('text');
                    SendMessage(g.objectName, g.pasteFuncName, str);
                });
            }
        };
    },
    passCopyToBrowser: function (stringPtr) {
        const g = window.becauseUnityIsBadWithJavascript_webglCopyAndPaste;
        const str = UTF8ToString(stringPtr);
        g.clipboardStr = str;
    },
});