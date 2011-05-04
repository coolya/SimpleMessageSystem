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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMS;


namespace SimpleMessageSystemTest
{
    [TestClass]
    public class MessengerTest
    {
        [TestMethod]
        public void SimpleSubscribeTest()
        {
            Messenger.Subscribe<string>(TestStringHandler);
            Messenger.GetMessage<string>().AddContent("Test Message").Publish();
            Messenger.Unsubscribe<string>(TestStringHandler);
        }

        [TestMethod]
        public void SimpleUnsubscribeTest()
        {
            Messenger.Subscribe<string>(TestFailHandler);
            Messenger.Unsubscribe<string>(TestFailHandler);
            Messenger.GetMessage<string>().AddContent("Test Message").Publish();            
        }

        private void TestFailHandler(Message<string> msg)
        {
            Assert.Fail();
        }

        private void TestStringHandler(Message<string> msg)
        {
            Assert.AreEqual("Test Message", msg.Content);
        }
    }
}
