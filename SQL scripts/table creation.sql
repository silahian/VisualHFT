USE [hft]
GO
/****** Object:  Table [dbo].[CloseExecutions]    Script Date: 10/6/2022 4:07:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CloseExecutions](
	[ExecutionID] [int] IDENTITY(1,1) NOT NULL,
	[PositionID] [bigint] NOT NULL,
	[ClOrdId] [varchar](50) NOT NULL,
	[ExecID] [varchar](150) NOT NULL,
	[LocalTimeStamp] [datetime2](7) NOT NULL,
	[ServerTimeStamp] [datetime2](7) NOT NULL,
	[Price] [decimal](18, 6) NULL,
	[ProviderID] [int] NOT NULL,
	[QtyFilled] [decimal](18, 2) NULL,
	[Side] [int] NULL,
	[Status] [int] NULL,
	[IsOpen] [bit] NOT NULL,
 CONSTRAINT [PK_CloseExecutions] PRIMARY KEY CLUSTERED 
(
	[ExecutionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CloseExecutionsLog]    Script Date: 10/6/2022 4:07:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CloseExecutionsLog](
	[ExecutionID] [int] IDENTITY(1,1) NOT NULL,
	[PositionID] [bigint] NOT NULL,
	[ClOrdId] [varchar](50) NOT NULL,
	[ExecID] [varchar](150) NOT NULL,
	[LocalTimeStamp] [datetime2](7) NOT NULL,
	[ServerTimeStamp] [datetime2](7) NOT NULL,
	[Price] [decimal](18, 6) NULL,
	[ProviderID] [int] NOT NULL,
	[QtyFilled] [decimal](18, 2) NULL,
	[Side] [int] NULL,
	[Status] [int] NULL,
	[IsOpen] [bit] NOT NULL,
 CONSTRAINT [PK_CloseExecutionsLog] PRIMARY KEY CLUSTERED 
(
	[ExecutionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OpenExecutions]    Script Date: 10/6/2022 4:07:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OpenExecutions](
	[ExecutionID] [int] IDENTITY(1,1) NOT NULL,
	[PositionID] [bigint] NOT NULL,
	[ClOrdId] [varchar](50) NOT NULL,
	[ExecID] [varchar](150) NOT NULL,
	[LocalTimeStamp] [datetime2](7) NOT NULL,
	[ServerTimeStamp] [datetime2](7) NOT NULL,
	[Price] [decimal](18, 6) NULL,
	[ProviderID] [int] NOT NULL,
	[QtyFilled] [decimal](18, 2) NULL,
	[Side] [int] NULL,
	[Status] [int] NULL,
	[IsOpen] [bit] NOT NULL,
 CONSTRAINT [PK_OpenExecutions] PRIMARY KEY CLUSTERED 
(
	[ExecutionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Positions]    Script Date: 10/6/2022 4:07:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Positions](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[PositionID] [bigint] NOT NULL,
	[AttemptsToClose] [int] NOT NULL,
	[CloseClOrdId] [varchar](50) NOT NULL,
	[CloseProviderId] [int] NOT NULL,
	[CloseQuoteId] [int] NULL,
	[CloseQuoteLocalTimeStamp] [datetime2](7) NULL,
	[CloseQuoteServerTimeStamp] [datetime2](7) NULL,
	[CloseStatus] [int] NOT NULL,
	[CloseTimeStamp] [datetime2](7) NOT NULL,
	[CreationTimeStamp] [datetime2](7) NOT NULL,
	[Currency] [varchar](10) NULL,
	[FreeText] [varchar](max) NULL,
	[FutSettDate] [date] NULL,
	[GetCloseAvgPrice] [decimal](18, 6) NOT NULL,
	[GetCloseQuantity] [decimal](18, 2) NOT NULL,
	[GetOpenAvgPrice] [decimal](18, 6) NOT NULL,
	[GetOpenQuantity] [decimal](18, 2) NOT NULL,
	[GetPipsPnL] [decimal](18, 2) NOT NULL,
	[IsCloseMM] [bit] NOT NULL,
	[IsOpenMM] [bit] NOT NULL,
	[MaxDrowdown] [decimal](18, 2) NOT NULL,
	[OpenClOrdId] [varchar](50) NOT NULL,
	[OpenProviderId] [int] NOT NULL,
	[OpenQuoteId] [int] NULL,
	[OpenQuoteLocalTimeStamp] [datetime2](7) NULL,
	[OpenQuoteServerTimeStamp] [datetime2](7) NULL,
	[OpenStatus] [int] NOT NULL,
	[OrderQuantity] [decimal](18, 2) NOT NULL,
	[PipsTrail] [decimal](18, 2) NOT NULL,
	[Side] [int] NOT NULL,
	[StopLoss] [decimal](18, 6) NOT NULL,
	[StrategyCode] [varchar](50) NOT NULL,
	[Symbol] [varchar](50) NOT NULL,
	[SymbolDecimals] [int] NOT NULL,
	[SymbolMultiplier] [int] NOT NULL,
	[TakeProfit] [decimal](18, 6) NOT NULL,
	[UnrealizedPnL] [decimal](18, 2) NOT NULL,
	[OpenBestBid] [decimal](18, 6) NULL,
	[OpenBestAsk] [decimal](18, 6) NULL,
	[CloseBestBid] [decimal](18, 6) NULL,
	[CloseBestAsk] [decimal](18, 6) NULL,
	[OpenOriginPartyID] [varchar](50) NULL,
	[CloseOriginPartyID] [varchar](50) NULL,
	[LayerName] [varchar](50) NULL,
	[OpenFireSignalTimestamp] [datetime2](7) NULL,
	[CloseFireSignalTimestamp] [datetime2](7) NULL,
	[PipsPnLInCurrency] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Positions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Providers]    Script Date: 10/6/2022 4:07:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Providers](
	[ProviderCode] [int] NOT NULL,
	[ProviderName] [varchar](501) NOT NULL,
 CONSTRAINT [PK_Providers] PRIMARY KEY CLUSTERED 
(
	[ProviderCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[STRATEGY_PARAMETERS_FIRMMM]    Script Date: 10/6/2022 4:07:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[STRATEGY_PARAMETERS_FIRMMM](
	[StrategyParamID] [int] IDENTITY(1,1) NOT NULL,
	[Symbol] [varchar](50) NOT NULL,
	[LayerName] [varchar](50) NULL,
	[PositionSize] [decimal](18, 2) NOT NULL,
	[MaximumExposure] [decimal](18, 2) NOT NULL,
	[LookUpBookForSize] [decimal](18, 2) NOT NULL,
	[PipsMarkupBid] [decimal](18, 2) NOT NULL,
	[PipsMarkupAsk] [decimal](18, 2) NOT NULL,
	[MinPipsDiffToUpdatePrice] [decimal](18, 2) NOT NULL,
	[MinSpread] [decimal](18, 2) NOT NULL,
	[PipsSlippage] [decimal](18, 2) NOT NULL,
	[AggressingToHedge] [bit] NOT NULL,
	[PipsSlippageToHedge] [decimal](18, 2) NOT NULL,
	[PipsHedgeTakeProf] [decimal](18, 2) NOT NULL,
	[PipsHedgeStopLoss] [decimal](18, 2) NOT NULL,
	[PipsHedgeTrailing] [bit] NOT NULL,
	[TickSample] [int] NOT NULL,
	[BollingerPeriod] [int] NOT NULL,
	[BollingerStdDev] [decimal](18, 2) NOT NULL,
	[PricingType] [int] NOT NULL,
 CONSTRAINT [PK_STRATEGY_PARAMETERS_FIRMMM] PRIMARY KEY CLUSTERED 
(
	[StrategyParamID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CloseExecutions]  WITH CHECK ADD  CONSTRAINT [FK_CloseExecutions_Positions] FOREIGN KEY([PositionID])
REFERENCES [dbo].[Positions] ([ID])
GO
ALTER TABLE [dbo].[CloseExecutions] CHECK CONSTRAINT [FK_CloseExecutions_Positions]
GO
ALTER TABLE [dbo].[OpenExecutions]  WITH CHECK ADD  CONSTRAINT [FK_OpenExecutions_Positions] FOREIGN KEY([PositionID])
REFERENCES [dbo].[Positions] ([ID])
GO
ALTER TABLE [dbo].[OpenExecutions] CHECK CONSTRAINT [FK_OpenExecutions_Positions]
GO
