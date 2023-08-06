
namespace demoTradingCore.Models
{
    public enum eEXCHANGE
    {
        PRIMEXM = 0,
        FASTMATCH_LL = 1,
        FXALL = 2,
        FLEXTRADE = 3,
        CURRENEX = 4,
        BITSTAMP = 5,
        LMAX = 6,
        CURRENEX_LL = 7,
        SWISSQUOTE = 8,
        ONETRADE = 9,
        HOTSPOT = 10,
        INTEGRAL = 11,
        FXCM = 12,
        CME_TT = 13,
        COINBASE = 14,
        CELER = 15,
        PRICEMARKETS = 16,
        SIMULATOR = 17,
        HFTACCEPTOR = 18,
        SPOTEX = 19,
        XENFIN = 20,
        DROPCOPYWEALTHMART = 21,
        DROPCOPYJCFX = 22,
        BINANCE = 23,
        ONEZERO = 24,
        ONEZERO_2 = 25,
        LMAX_ITCH = 26,
        CFH = 27,
        PRIMEXM_2 = 28,
        GTX = 29,
        FASTMATCH = 30,
        CBOE_FX = 31,
        SOR = 32,
        EUREX = 33,
        BINANCEWS = 34,
        ISPRIME = 35,
        BOSONIC = 36,
        DIGITAL_ECN = 37,
        XTRD = 38,
        FASTMATCH_2 = 39,
        KUCOIN = 40,
        ALPACA = 41,
        GEMINI = 42,
        CRYPTOCOM = 43,
        ONEZERO_3 = 44,
        ONEZERO_4 = 45,
        ONEZERO_5 = 46,
        ONEZERO_6 = 47,
        OKEX = 48
    };

    public enum eINCREMENTALTYPE
    {
        CHANGEITEM,
        DELETEITEM,
        NEWITEM
    };
    public enum eSESSIONSTATUS
    {
        PRICE_CONNECTED_ORDER_DISCONNECTED,
        PRICE_DSICONNECTED_ORDER_CONNECTED,
        BOTH_CONNECTED,
        BOTH_DISCONNECTED
    };
}