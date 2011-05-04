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
        public T Content { get; set; }

        internal bool _inUse = false;
        /// <summary>
        /// count of subscribers currently dealing with the message.
        /// </summary>
        private int _handles = 0;

        internal Message() { }

        internal Message(T content)
        {
            Content = content;
        }

        internal Message<T> Increment()
        {
            _handles++;
            return this;
        }

        internal Message<T> Decrement()
        {
            _handles--;
            return this;
        }

        public Message<T> AddContent(T content)
        {
            this.Content = content;
            return this;
        }

        public void Publish()
        {
            Messenger.SendMessage<T>(this);
        }

        public void Recycle()
        {
            int retries = 1000;

            while (_handles > 0 && retries > 0)
            {
                retries--;
                System.Threading.Thread.Sleep(10);
            }

            if (retries == 0 && _handles > 0)
                throw new InvalidOperationException(
                    "You tried to resycle a Message that is currently in use by at leat one Handler.");

            _inUse = false;
        }
    }
}
