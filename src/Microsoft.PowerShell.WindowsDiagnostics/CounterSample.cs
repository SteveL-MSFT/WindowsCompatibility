//
// Copyright (c) Microsoft Corporation. All rights reserved.
//


using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Resources;
using Microsoft.Powershell.Commands.GetCounter.PdhNative;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;


namespace Microsoft.PowerShell.Commands.GetCounter
{
    /// <summary>
    /// PerformanceCounterSample
    /// </summary>
    public class PerformanceCounterSample
    {
        internal PerformanceCounterSample()
        {
        }

        internal PerformanceCounterSample(string path,
                               string instanceName,
                               double cookedValue,
                               UInt64 rawValue,
                               UInt64 secondValue,
                               uint multiCount,
                               PerformanceCounterType counterType,
                               UInt32 defaultScale,
                               UInt64 timeBase,
                               DateTime timeStamp,
                               UInt64 timeStamp100nSec,
                               UInt32 status)
        {
            _path = path;
            _instanceName = instanceName;
            _cookedValue = cookedValue;
            _rawValue = rawValue;
            _secondValue = secondValue;
            _multiCount = multiCount;
            _counterType = counterType;
            _defaultScale = defaultScale;
            _timeBase = timeBase;
            _timeStamp = timeStamp;
            _timeStamp100nSec = timeStamp100nSec;
            _status = status;
        }

        /// <summary>
        /// Path
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        private string _path = "";

        /// <summary>
        /// InstanceName
        /// </summary>
        public string InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }
        private string _instanceName = "";

        /// <summary>
        /// CookedValue
        /// </summary>
        public double CookedValue
        {
            get { return _cookedValue; }
            set { _cookedValue = value; }
        }
        private double _cookedValue = 0;

        /// <summary>
        /// RawValue
        /// </summary>
        public UInt64 RawValue
        {
            get { return _rawValue; }
            set { _rawValue = value; }
        }
        private UInt64 _rawValue = 0;

        /// <summary>
        /// SecondValue
        /// </summary>
        public UInt64 SecondValue
        {
            get { return _secondValue; }
            set { _secondValue = value; }
        }
        private UInt64 _secondValue = 0;

        /// <summary>
        /// MultipleCount
        /// </summary>
        public uint MultipleCount
        {
            get { return _multiCount; }
            set { _multiCount = value; }
        }
        private uint _multiCount = 0;

        /// <summary>
        /// CounterType
        /// </summary>
        public PerformanceCounterType CounterType
        {
            get { return _counterType; }
            set { _counterType = value; }
        }
        private PerformanceCounterType _counterType = 0;

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        private DateTime _timeStamp = DateTime.MinValue;

        /// <summary>
        /// Timestamp100NSec
        /// </summary>
        public UInt64 Timestamp100NSec
        {
            get { return _timeStamp100nSec; }
            set { _timeStamp100nSec = value; }
        }
        private UInt64 _timeStamp100nSec = 0;

        /// <summary>
        /// Status
        /// </summary>
        public UInt32 Status
        {
            get { return _status; }
            set { _status = value; }
        }
        private UInt32 _status = 0;

        /// <summary>
        /// DefaultScale
        /// </summary>
        public UInt32 DefaultScale
        {
            get { return _defaultScale; }
            set { _defaultScale = value; }
        }
        private UInt32 _defaultScale = 0;

        /// <summary>
        /// TimeBase
        /// </summary>
        public UInt64 TimeBase
        {
            get { return _timeBase; }
            set { _timeBase = value; }
        }
        private UInt64 _timeBase = 0;
    }

    /// <summary>
    /// PerformanceCounterSampleSet
    /// </summary>
    public class PerformanceCounterSampleSet
    {
        internal PerformanceCounterSampleSet()
        {
            _resourceMgr = Microsoft.PowerShell.Commands.Diagnostics.Common.CommonUtilities.GetResourceManager();
        }

        internal PerformanceCounterSampleSet(DateTime timeStamp,
                                    PerformanceCounterSample[] counterSamples,
                                    bool firstSet) : this()
        {
            _timeStamp = timeStamp;
            _counterSamples = counterSamples;
        }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        private DateTime _timeStamp = DateTime.MinValue;

        /// <summary>
        /// CounterSamples
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
                    Scope = "member",
                    Target = "Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSample.CounterSamples",
                    Justification = "A string[] is required here because that is the type Powershell supports")]
        public PerformanceCounterSample[] CounterSamples
        {
            get { return _counterSamples; }
            set { _counterSamples = value; }
        }
        private PerformanceCounterSample[] _counterSamples = null;

        private ResourceManager _resourceMgr = null;
    }
}
