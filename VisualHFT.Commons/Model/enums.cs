using System.ComponentModel;


public enum ePOSITIONSIDE { Buy, Sell, None };
public enum ePOSITIONSTATUS { NONE, SENT, NEW, CANCELED, REJECTED, PARTIALFILLED, FILLED, CANCELEDSENT, REPLACESENT, REPLACED, LASTLOOK_HOLDING };
public enum eTIMEINFORCE { FOK, IOC, GTC };
public enum eINFORMEDSTATUS { INFORMEDSTATUS_NONE, INFORMEDSTATUS_SENT, INFORMEDSTATUS_ACK };

public enum eORDERSIDE {
    Buy,
    Sell,
    None
};
public enum eORDERSTATUS { NONE, SENT, NEW, CANCELED, REJECTED, PARTIALFILLED, FILLED, CANCELEDSENT, REPLACESENT, REPLACED, LASTLOOK_HOLDING };
public enum eORDERTIMEINFORCE { NONE, GTC, IOC, FOK };
public enum eORDERTYPE { LIMIT, MARKET, PEGGED, NONE };
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
    PRICE_CONNECTED_ORDER_DISCONNECTED,
    PRICE_DSICONNECTED_ORDER_CONNECTED,
    BOTH_CONNECTED,
    BOTH_DISCONNECTED
};


public enum AggregationLevel
{
    [Description("No Aggregation")]
    None,

    [Description("1 Millisecond")]
    Ms1,

    [Description("10 Milliseconds")]
    Ms10,

    [Description("100 Milliseconds")]
    Ms100,

    [Description("500 Milliseconds")]
    Ms500,

    [Description("1 Second")]
    S1,

    [Description("3 Seconds")]
    S3,

    [Description("5 Seconds")]
    S5,

    [Description("Automatic Aggregation")]
    Automatic
}

public enum PositionManagerCalculationMethod
{
    FIFO,
    LIFO
}
