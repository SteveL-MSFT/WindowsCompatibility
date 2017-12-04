//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

namespace Microsoft.PowerShell.Commands.GetCounter
{

    /// <summary>
    /// CounterFileInfo
    /// </summary>
    public class CounterFileInfo
    {
        internal CounterFileInfo(DateTime oldestRecord,
            DateTime newestRecord,
            UInt32 sampleCount)
        {
            _oldestRecord = oldestRecord;
            _newestRecord = newestRecord;
            _sampleCount = sampleCount;
        }

        internal CounterFileInfo() { }

        /// <summary>
        /// OldestRecord
        /// </summary>
        public DateTime OldestRecord
        {
            get
            {
                return _oldestRecord;
            }
        }
        private DateTime _oldestRecord = DateTime.MinValue;

        /// <summary>
        /// NewestRecord
        /// </summary>
        public DateTime NewestRecord
        {
            get
            {
                return _newestRecord;
            }
        }
        private DateTime _newestRecord = DateTime.MaxValue;

        /// <summary>
        /// SampleCount
        /// </summary>
        public UInt32 SampleCount
        {
            get
            {
                return _sampleCount;
            }
        }
        private UInt32 _sampleCount = 0;
    }
}

