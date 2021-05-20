﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM {
    public static class TransitionOnKey {
        
        public class Down : TransitionBase {
            private KeyCode keyCode;

            /// <summary>
            /// Initialises a new transition that triggers, while a key is down.
            /// It behaves like Input.GetKey(...).
            /// </summary>
            /// <param name="key">The KeyCode of the key to watch</param>
            public Down(
                    string from, 
                    string to, 
                    KeyCode key,
                    bool forceInstantly = false) : base(from, to, forceInstantly) {

                keyCode = key;
            }

            public override bool ShouldTransition() {
                return Input.GetKey(keyCode);
            }
        }

        /// <summary>
        /// Initialises a new transition that triggers, when a key was just down and is up now.
        /// It behaves like Input.GetKeyUp(...).
        /// </summary>
        /// <param name="key">The KeyCode of the key to watch</param>
        public class Release : TransitionBase {
            private KeyCode keyCode;

            public Release(
                    string from, 
                    string to, 
                    KeyCode key,
                    bool forceInstantly = false) : base(from, to, forceInstantly) {

                keyCode = key;
            }

            public override bool ShouldTransition() {
                return Input.GetKeyUp(keyCode);
            }
        }

        /// <summary>
        /// Initialises a new transition that triggers, when a key was just up and is down now.
        /// It behaves like Input.GetKeyDown(...).
        /// </summary>
        /// <param name="key">The KeyCode of the key to watch</param>
        public class Press : TransitionBase {
            private KeyCode keyCode;

            public Press(
                    string from, 
                    string to, 
                    KeyCode key,
                    bool forceInstantly = false) : base(from, to, forceInstantly) {

                keyCode = key;
            }

            public override bool ShouldTransition() {
                return Input.GetKeyDown(keyCode);
            }
        }

        /// <summary>
        /// Initialises a new transition that triggers, while a key is up.
        /// It behaves like ! Input.GetKey(...).
        /// </summary>
        /// <param name="key">The KeyCode of the key to watch</param>
        public class Up : TransitionBase {
            private KeyCode keyCode;

            public Up(
                    string from, 
                    string to, 
                    KeyCode key,
                    bool forceInstantly = false) : base(from, to, forceInstantly) {

                keyCode = key;
            }

            public override bool ShouldTransition() {
                return ! Input.GetKey(keyCode);
            }
        }
    }
}
