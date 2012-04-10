/*
* XSockets.NET JavaScript Library v1.0.2.beta
* http://xsockets.net/
* Distributed in whole under the terms of the MIT
*
* Copyright 2012, Magnus Thor & Ulf Björklund
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
* LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
* OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
* WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*/
(function () {
    "use strict";
    var jXSockets = {
        xWebSocket: function (url, subprotocol) {
            /// <summary>
            /// XSockets.NET JavaScript API 
            /// </summary>
            ///<param name="url" type="String">
            /// The WebSocket handler (URL) to connect to. i.e ws://127.0.0.1:4502/GenericText
            ///</param>
            ///<param name="subprotocol" type="String">
            ///     Subprotocol i.e Chat 
            ///</param>                            
            var webSocket = null;
            var subscriptions = [];

            var self = this;

            if ('WebSocket' in window) {
                webSocket = new WebSocket(url, subprotocol);
            } else if ('MozWebSocket' in window) {
                webSocket = new MozWebSocket(url, subprotocol);
            } else {
                webSocket = null;
            }
            if (webSocket !== null) {
                webSocket.onclose = function (msg) {
                    dispatch('close', msg);
                };
                webSocket.onopen = function (msg) {
                    dispatch('open', msg);
                };
                webSocket.onmessage = function (message) {
                    raiseEvent(message);
                };
            }
            this.bind = function (event, fn, options) {
                /// <summary>
                ///     Attach a handler (subscription) for the current WebSocket Handler
                /// </summary>
                /// <param name="event" type="string">
                ///    Name of the event to subscribe to (bind)
                /// </param>           
                /// <param name="fn" type="function">
                ///    A function to execute each time the event (subscription) is triggered.
                /// </param>   
                /// <param name="options" type="object">
                ///    N/A
                /// </param>  
                var o = {
                    callback: fn,
                    options: null
                };

                subscriptions[event] = subscriptions[event] || [];
                subscriptions[event].push(o);

            };

            this.unbind = function (event, callback) {
                /// <summary>
                ///     Remove a previously-attached event handler (subscription).
                /// </summary>
                /// <param name="event" type="String">
                ///    Name of the event (subscription) to unbind.
                /// </param>           
                /// <param name="callback" type="function">
                ///    A function to execute when completed.
                /// </param>                                   
                for (var i = 0; i < subscriptions[event].length; i++) {
                    subscriptions[event].splice(i, 1);
                }
                if (callback && typeof (callback) === "function") {
                    callback();
                }                
            };
            this.many = function (event, count, callback) {
                /// <summary>
                ///     Attach a handler to an event (subscription) for the current WebSocket Handler,  unbinds when the event is triggered the specified number of (count) times.
                /// </summary>
                /// <param name="event" type="String">
                ///    Name of the event (subscription)
                /// </param>           
                /// <param name="count" type="Number">
                ///     Number of times to listen to this event (subscription)
                /// </param>           
                /// <param name="callback" type="Function">
                ///    A function to execute at the time the event is triggered the specified number of times.
                /// </param> 
                subscriptions[event] = subscriptions[event] || [];
                subscriptions[event].push({
                    callback: callback,
                    options: {
                        counter: {
                            messages: count,
                            completed: function () {
                                for (var i = 0; i < subscriptions[event].length; i++) {
                                    subscriptions[event].splice(i, 1);
                                    break;
                                }
                            }
                        }
                    }
                });
            };
            this.one = function (event, callback) {
                /// <summary>
                ///    Attach a handler to an event (subscription) for the current WebSocket Handler. The handler is executed at most once.
                /// </summary>
                /// <param name="event" type="String">
                ///    Name of the event (subscription)
                /// </param>           
                /// <param name="callback" type="Function">
                ///    A function to trigger when executed once.
                /// </param>           
                subscriptions[event] = subscriptions[event] || [];
                subscriptions[event].push({
                    callback: callback,
                    options: {
                        counter: {
                            messages: 1,
                            completed: function () {
                                for (var i = 0; i < subscriptions[event].length; i++) {
                                    subscriptions[event].splice(i, 1);
                                }
                            }
                        }
                    }
                });
            };
            this.trigger = function (event, json, callback) {
                /// <summary>
                ///      Trigger (Publish)  a WebSocketMessage (event) to the current WebSocket Handler.
                /// </summary>
                /// <param name="event" type="string">
                ///     Name of the event (publish)
                /// </param>                
                /// <param name="json" type="JSON">
                ///     JSON representation of the WebSocketMessage to trigger/send (publish)
                /// </param>                
                /// <param name="callback" type="function">
                ///      A function to execute when completed. 
                /// </param>                                
                if (typeof (event) !== "object") {
                    var msg = new jXSockets.WebSocketMessage(event);
                    msg.AddJson(json);
                    send(msg.toString());
                    if (callback && typeof (callback) === "function") {
                        callback();
                    }
                } else {
                    send(JSON.stringify(event));
                    if (json && typeof (json) === "function") {
                        json();
                    }
                }
            };
            this.send = function (payload) {
                /// <summary>
                ///     Send a binary message to the current WebSocket Handler
                /// </summary>
                /// <param name="payload" type="Blob">
                ///     Binary object to send (Blob/ArrayBuffer).
                /// </param>                
                send(payload);
            };

            var dispatch = function (event_name, message) {
                var chain = subscriptions[event_name];
                if (typeof chain === 'undefined') { return; }
                for (var i = 0; i < chain.length; i++) {
                    chain[i].callback(message);
                    var opts = chain[i].options;
                    if (opts !== null) {
                        if (opts.counter !== undefined) {
                            chain[i].options.counter.messages--;
                            if (chain[i].options.counter.messages === 0) {
                                if (typeof opts.counter.completed !== 'undefined') {
                                    opts.counter.completed();
                                }
                            }
                        }
                    }
                }
            };
            var raiseEvent = function (message) {
                var msg = {}, event = null;
                if (typeof message.data === "string") {
                    msg = JSON.parse(message.data);
                    event = msg.event;
                    dispatch(event, msg.data);
                } else {
                    dispatch("blob", message.data);
                }
            };
            var send = function (payload) {
                if (webSocket.bufferedAmount === 0 && webSocket.readyState === 1) {
                    dispatch("send", {
                        bufferedAmount: webSocket.bufferedAmount
                    });
                    webSocket.send(payload);
                }
            };
            return this;
        },
        WebSocketMessage: function (eventName) {
            /// <summary>
            /// Create a new WebSocketMessage event
            /// </summary>
            ///<param name="eventName" type="string">
            /// Name of the event (Message)
            ///</param>
            this.EventName = eventName;
            this.Message = {
                event: eventName,
                data: null
            };
            this.AddJson = function (obj) {
                /// <summary>
                /// Add a JSON object to the WebSocketMessage
                /// </summary>
                ///<param name="obj" type="json">
                /// JSON object to add to the message
                ///</param>
                this.Message.data = obj;
                return this;
            };
            this.AddJsonString = function (jsonStr) {
                /// <summary>
                /// Add a JSON (string) to the WebSocketMessage
                /// </summary>
                ///<param name="jsonStr" type="string">
                /// The string representation of the JSON object to be parsed and added as JSON to the WebSocketMessage
                ///</param>
                this.Message.Data = JSON.parse(jsonStr);
                return this.Message;
            };
            this.toString = function toString() {
                /// <summary>
                /// Get the the string representation of the WebSocketMessage
                /// </summary>
                return JSON.stringify(this.Message);
            };
            this.PayLoad = function () {
                /// <summary>
                /// Get WebSocketMessage (JSON)
                /// </summary>
                return this.Message;
            };
            return this;
        }
    };
    if (!window.jXSockets) {
        window.jXSockets = jXSockets;
    }
    if (!window.$$) {
        window.$$ = jXSockets;
    }
})();