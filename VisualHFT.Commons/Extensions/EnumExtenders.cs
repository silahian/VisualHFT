using VisualHFT.Enums;
using VisualHFT.PluginManager;

namespace VisualHFT.Commons.Extensions
{

    public static class EnumExtensions
    {

        public static ePluginStatus ToPluginStatus(this eSESSIONSTATUS status)
        {
            switch (status)
            {
                case eSESSIONSTATUS.CONNECTING:
                    return ePluginStatus.STARTING;
                case eSESSIONSTATUS.CONNECTED:
                    return ePluginStatus.STARTED;
                case eSESSIONSTATUS.CONNECTED_WITH_WARNINGS:
                    return ePluginStatus.MALFUNCTIONING;
                case eSESSIONSTATUS.DISCONNECTED_FAILED:
                    return ePluginStatus.STOPPED_FAILED;
                case eSESSIONSTATUS.DISCONNECTED:
                    return ePluginStatus.STOPPED;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static eSESSIONSTATUS ToSessionStatus(this ePluginStatus status)
        {
            switch (status)
            {
                case ePluginStatus.LOADING:
                    return eSESSIONSTATUS.DISCONNECTED;
                case ePluginStatus.LOADED:
                    return eSESSIONSTATUS.DISCONNECTED;
                case ePluginStatus.STARTING:
                    return eSESSIONSTATUS.CONNECTING;
                case ePluginStatus.STARTED:
                    return eSESSIONSTATUS.CONNECTED;
                case ePluginStatus.STOPPED:
                    return eSESSIONSTATUS.DISCONNECTED;
                case ePluginStatus.STOPPED_FAILED:
                    return eSESSIONSTATUS.DISCONNECTED_FAILED;
                case ePluginStatus.MALFUNCTIONING:
                    return eSESSIONSTATUS.CONNECTED_WITH_WARNINGS;
                case ePluginStatus.STOPPING:
                    return eSESSIONSTATUS.DISCONNECTED;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);

            }
        }
    }

}
