﻿using System.ComponentModel;

namespace VisualHFT.Enums
{
    public enum ePOSITIONSIDE
    {
        Buy,
        Sell,
        None
    };

    public enum ePOSITIONSTATUS
    {
        NONE,
        SENT,
        NEW,
        CANCELED,
        REJECTED,
        PARTIALFILLED,
        FILLED,
        CANCELEDSENT,
        REPLACESENT,
        REPLACED,
        LASTLOOK_HOLDING
    };

    public enum eTIMEINFORCE
    {
        FOK,
        IOC,
        GTC
    };

    public enum eINFORMEDSTATUS
    {
        INFORMEDSTATUS_NONE,
        INFORMEDSTATUS_SENT,
        INFORMEDSTATUS_ACK
    };

    public enum eORDERSIDE
    {
        Buy,
        Sell,
        None
    };

    public enum eLOBSIDE
    {
        BID,
        ASK,
        NONE
    }

    public enum eORDERSTATUS
    {
        NONE,
        SENT,
        NEW,
        CANCELED,
        REJECTED,
        PARTIALFILLED,
        FILLED,
        CANCELEDSENT,
        REPLACESENT,
        REPLACED,
        LASTLOOK_HOLDING
    };

    public enum eORDERTIMEINFORCE
    {
        NONE,
        GTC,
        IOC,
        FOK,
        MOK
    };

    public enum eORDERTYPE
    {
        LIMIT,
        MARKET,
        PEGGED,
        NONE
    };

    public enum ePRICINGTYPE
    {
        BASED_ON_CURRENT_BIDASK,
        BASED_ON_OTHER_SIDE,
        BASED_ON_MIDPRICE,
        BASED_ON_MEANREVERSION,
        BASED_ON_ORDERBOOK
    };

    public enum eSESSIONSTATUS
    {
        CONNECTING,
        CONNECTED,
        CONNECTED_WITH_WARNINGS,
        DISCONNECTED_FAILED,
        DISCONNECTED,
    };


    public enum AggregationLevel
    {

        [Description("No Aggregation")] None,

        [Description("1 Millisecond")] Ms1,

        [Description("10 Milliseconds")] Ms10,

        [Description("100 Milliseconds")] Ms100,

        [Description("500 Milliseconds")] Ms500,

        [Description("1 Second")] S1,

        [Description("3 Seconds")] S3,

        [Description("5 Seconds")] S5,

        [Description("Daily")] D1,

        //[Description("Automatic Aggregation")] Automatic
    }

    public enum PositionManagerCalculationMethod
    {
        FIFO,
        LIFO
    }

    public enum eMDUpdateAction
    {
        New,
        Change,
        Delete,
        Change_Adjust,
        Replace,
        None
    }

    public enum ePluginType
    {
        UNKNOWN,
        STUDY,
        MULTI_STUDY,
        MARKET_CONNECTOR,
    }

}