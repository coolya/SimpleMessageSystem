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
using System.Threading.Tasks;

namespace SMS
{

    public static class Messenger
    {
        private static IDictionary<Type, ICollection<object>> _subscribers =
            new Dictionary<Type, ICollection<object>>();

        /// <summary>
        /// Obains a new Message for <code>TMessage</code> this can be a new Message a unused 
        /// Message from the internal pool
        /// </summary>
        /// <typeparam name="TMessage">Type of data the message should wrap</typeparam>
        /// <returns>a (new) Message that can be used</returns>
        public static Message<TMessage> GetMessage<TMessage>()
        {
            Message<TMessage> message = MessagePool.GetMessage<TMessage>();

            if (message == null)
            {
                message = new Message<TMessage>();
                message._inUse = true;
                MessagePool.AddMessage<TMessage>(message);
            }

            return message;
        }

        public static Message<TMessage> GetMessage<TMessage>(TMessage content)
        {
            Message<TMessage> message = MessagePool.GetMessage<TMessage>();

            if (message == null)
            {
                message = new Message<TMessage>();
                message._inUse = true;
                MessagePool.AddMessage<TMessage>(message);
            }

            message.Content = content;

            return message;
        }

        public static void Subscribe<TMesseage>(Action<Message<TMesseage>> handler)
        {
            if (!_subscribers.ContainsKey(typeof(TMesseage)))
            {
                _subscribers.Add(typeof(TMesseage), new List<object>());
            }
            _subscribers[typeof(TMesseage)].Add(handler);
        }
        
        public static void Subscribe<TMesseage>(IMessageHandler<TMesseage> handler)
        {
            Subscribe<TMesseage>(handler.MessageArrived);
        }

        public static void Unsubscribe<TMesseage>(Action<Message<TMesseage>> handler)
        {
            if (_subscribers.ContainsKey(typeof(TMesseage)))
            {
                _subscribers[typeof(TMesseage)].Remove(handler);
            }            
        }

        public static void Unsubscribe<TMesseage>(IMessageHandler<TMesseage> handler)
        {
            Unsubscribe<TMesseage>(handler.MessageArrived);
        }

        public static void SendMessage<TMessage>(Message<TMessage> msg)
        {
            Type targetMessagetype = typeof(TMessage);

            if (_subscribers.ContainsKey(targetMessagetype))
            {
                List<Task> tasks = new List<Task>();
                foreach (var item in _subscribers[targetMessagetype])
                {
                    //increment handles to be sure no one is working with the message when we recycle it
                    msg.Increment();
                    //notify subscribers in a new thread for concurrency
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        ((Action<Message<TMessage>>)item)(msg);
                        //decrement handle to prepare for recycle when all handlers have finished
                        msg.Decrement();
                    }));
                }

                if (msg.PublishComplete != null)
                {
                    //call back all the message when subscribers are finished
                    Task.Factory.StartNew(() =>
                        {
                            Task.WaitAll(tasks.ToArray());
                            msg.PublishComplete(msg);
                        }
                    );
                }
            }
        }
        
            private static class MessagePool
            {

                private static IDictionary<Type, ICollection<object>> _messagePool =
                    new Dictionary<Type, ICollection<object>>();

                private static object _lock = new object();

                public static Message<TMessage> GetMessage<TMessage>()
                {
                    Message<TMessage> ret = null;

                    lock (_lock)
                    {
                        if (_messagePool.ContainsKey(typeof(TMessage)))
                        {
                            ret = (Message<TMessage>)_messagePool[typeof(TMessage)].FirstOrDefault((item) =>
                                ((Message<TMessage>)item)._inUse == false);

                            if (ret != null)
                                ret._inUse = true;
                        }
                    }
                    return ret;
                }

                public static void AddMessage<TMessage>(Message<TMessage> message)
                {
                    if (!_messagePool.ContainsKey(typeof(TMessage)))
                    {
                        _messagePool.Add(typeof(TMessage), new List<object>());
                    }

                    _messagePool[typeof(TMessage)].Add(message);

                }
            }

        
    }
}
