﻿//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;

    class KeyboardThread
    {
        private readonly Thread _thread;

        public Queue<NativeMethods.INPUT> _events = new Queue<NativeMethods.INPUT>();
        public int _keyDelay = 30;

        public KeyboardThread(int keyDelay)
        {
            _keyDelay = keyDelay;
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        #region Properties

        public int KeyDelay
        {
            get
            {
                lock (typeof(KeyboardThread))
                {
                    return _keyDelay;
                }
            }
            set
            {
                lock (typeof(KeyboardThread))
                {
                    if (!_keyDelay.Equals(value) && value > 0)
                    {
                        _keyDelay = value;
                    }
                }
            }
        }

        #endregion

        internal void AddEvents(List<NativeMethods.INPUT> events)
        {
            lock (typeof(KeyboardThread))
            {
                bool interupt = (_events.Count == 0);

                foreach (NativeMethods.INPUT keyEvent in events)
                {
                    _events.Enqueue(keyEvent);
                }
                if (interupt)
                {
                    _thread.Interrupt();
                }
            }
        }

        public void Run()
        {
            /* Added functionality for a TCP server open on a static port of 5009
             * If there is a TCP client connected then it will send key presses to a differnt PC running the receiver
             * Otherwise it will send the keypresses to the local PC
             * 
             * Code may need a clean up as I used a routine I have used many times and know it works
             * We dont use much of the functionality the file provides but it was handy and a known quality
            */
            KeyboardTCPServer KeyboardTCPServer = KeyboardTCPServer.Instance;
            KeyboardTCPServer.Open();
            while (true)
            {

                int sleepTime = Timeout.Infinite;

                lock (typeof(KeyboardThread))
                {
                    if (_events.Count > 0)
                    {
                        sleepTime = _keyDelay;
                        NativeMethods.INPUT keyEvent = _events.Dequeue();

                        int size = Marshal.SizeOf(keyEvent);
                        byte[] arr = new byte[size];
                        IntPtr ptr = Marshal.AllocHGlobal(size);
                        Marshal.StructureToPtr(keyEvent, ptr, true);
                        Marshal.Copy(ptr, arr, 0, size);
                        
                        if (!KeyboardTCPServer.Send(arr, size))
                          NativeMethods.SendInput(1, new NativeMethods.INPUT[] { keyEvent }, Marshal.SizeOf(keyEvent));
                        Marshal.FreeHGlobal(ptr);
                    }
                }

                try
                {
                    Thread.Sleep(sleepTime);
                }
                catch (ThreadInterruptedException)
                {
                  // Close TCP Server cleanly
                  KeyboardTCPServer.Instance.Close();
                }
            }
        }
    }
}
