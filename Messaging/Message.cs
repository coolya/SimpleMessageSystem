/*
 * Copyright (C) 2011 Kolja Dummann <k.dummann@gmail.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMS
{
    public class Message<T>
    {
        /// <summary>
        /// callback when the message has been published and all subscribers finished their work
        /// </summary>
        public Action<Message<T>> PublishComplete { get; set; }

        public T Content { get; set; }

        internal bool _inUse = false;
        /// <summary>
        /// count of subscribers currently dealing with the message.
        /// </summary>
        private int _handles = 0;

        /// <summary>
        /// creates a new message
        /// </summary>
        internal Message() { }

        /// <summary>
        /// Creates a new message with content
        /// </summary>
        /// <param name="content">content for the message</param>
        internal Message(T content)
        {
            Content = content;
        }

        /// <summary>
        /// increments the handle count
        /// </summary>
        /// <returns>the message</returns>
        internal Message<T> Increment()
        {
            _handles++;
            return this;
        }

        /// <summary>
        /// decrements the handle count
        /// </summary>
        /// <returns>the message</returns>
        internal Message<T> Decrement()
        {
            _handles--;
            return this;
        }

        /// <summary>
        /// adds content to the message
        /// </summary>
        /// <param name="content">content for the message</param>
        /// <returns>the message with the content</returns>
        public Message<T> AddContent(T content)
        {
            this.Content = content;
            return this;
        }

        public Message<T> SetCallback(Action<Message<T>> callback)
        {
            if (this.PublishComplete != null)
                throw new InvalidOperationException("callback has already been set");

            this.PublishComplete = callback;

            return this;
        }

        public Message<T> ResetCallback()
        {
            this.PublishComplete = null;
            return this;
        }
        
        /// <summary>
        /// publishes the message over the messanger to the subscribers for this message type.
        /// calls to the subscribers are done in a new thread to gain max performance. 
        /// This call will return instant.
        /// </summary>
        /// <returns>the message</returns>
        public Message<T> Publish()
        {
            Messenger.SendMessage<T>(this);
            return this;
        }


        /// <summary>
        /// recycles the message that it can be used by other clients and gets back to the internal messagepool
        /// </summary>
        public void Recycle()
        {
            if (_handles > 0)
                throw new InvalidOperationException(
                    "tried to resycle a message that is currently in use by at least one handler.");

            _inUse = false;
        }
    }
}
