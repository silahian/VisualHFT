using System.Collections.Generic;
using CryptoExchange.Net.Converters;
using Gemini.Net.Enums;

namespace Gemini.Net.Converters
{
    internal class BizTypeConverter : BaseConverter<BizType>
    {
        private readonly bool _useCaps;

        public BizTypeConverter() : base(false) { }        

        public BizTypeConverter(bool useCaps) : this() { _useCaps = useCaps; }

        protected override List<KeyValuePair<BizType, string>> Mapping => new List<KeyValuePair<BizType, string>>
        {
            new KeyValuePair<BizType, string>(BizType.ConvertToKCS, _useCaps ? "" : "Convert to KCS"),
            new KeyValuePair<BizType, string>(BizType.Deposit, _useCaps ? "DEPOSIT" : "Deposit"),
            new KeyValuePair<BizType, string>(BizType.Exchange, _useCaps ? "TRADE_EXCHANGE" : "Exchange"),
            new KeyValuePair<BizType, string>(BizType.Trade, _useCaps ? "TRADE_EXCHANGE" : "Trade"),
            new KeyValuePair<BizType, string>(BizType.KCSPayFees, _useCaps ? "" : "KCS Pay Fees"),
            new KeyValuePair<BizType, string>(BizType.OtherRewards, _useCaps ? "" : "Other rewards"),
            new KeyValuePair<BizType, string>(BizType.RefundedFees, _useCaps ? "" : "Refunded Fees"),
            new KeyValuePair<BizType, string>(BizType.Rewards, _useCaps ? "" : "Rewards"),
            new KeyValuePair<BizType, string>(BizType.SoftStakingProfits, _useCaps ? "" : "Soft Staking Profits"),
            new KeyValuePair<BizType, string>(BizType.Staking, _useCaps ? "" : "Staking"),
            new KeyValuePair<BizType, string>(BizType.StakingProfits, _useCaps ? "" : "Staking Profits"),
            new KeyValuePair<BizType, string>(BizType.Transfer, _useCaps ? "TRANSFER" : "Transfer"),
            new KeyValuePair<BizType, string>(BizType.Withdrawal, _useCaps ? "WITHDRAW" : "Withdrawal"),

            new KeyValuePair<BizType, string>(BizType.AssetsTransferred, _useCaps ? "" : "Assets Transferred in After Upgrading"),
            new KeyValuePair<BizType, string>(BizType.VoteForCoin, _useCaps ? "" : "Vote for Coin"),
            new KeyValuePair<BizType, string>(BizType.GeminiBonus, _useCaps ? "KUCOIN_BONUS" : "Gemini Bonus"),
            new KeyValuePair<BizType, string>(BizType.ReferralBonus, _useCaps ? "" : "Referral Bonus"),
            new KeyValuePair<BizType, string>(BizType.Distribution, _useCaps ? "" : "Distribution"),
            new KeyValuePair<BizType, string>(BizType.AirdropFork, _useCaps ? "" : "Airdrop/Fork"),
            new KeyValuePair<BizType, string>(BizType.FeeRebate, _useCaps ? "" : "Fee Rebate"),
            new KeyValuePair<BizType, string>(BizType.BuyCrypto, _useCaps ? "" : "Buy Crypto"),
            new KeyValuePair<BizType, string>(BizType.SellCrypto, _useCaps ? "" : "Sell Crypto"),
            new KeyValuePair<BizType, string>(BizType.PublicOfferingPurchase, _useCaps ? "" : "Public Offering Purchase"),
            new KeyValuePair<BizType, string>(BizType.SendRedEnvelope, _useCaps ? "" : "Send red envelope"),
            new KeyValuePair<BizType, string>(BizType.OpenRedEnvelope, _useCaps ? "" : "Open red envelope"),
            new KeyValuePair<BizType, string>(BizType.LockDropVesting, _useCaps ? "" : "LockDrop Vesting"),
            new KeyValuePair<BizType, string>(BizType.Redemption, _useCaps ? "" : "Redemption"),
            new KeyValuePair<BizType, string>(BizType.MarginTrade, _useCaps ? "MARGIN_EXCHANGE" : "Margin Trade"),
            new KeyValuePair<BizType, string>(BizType.Loans, _useCaps ? "" : "Loans"),
            new KeyValuePair<BizType, string>(BizType.Borrowings, _useCaps ? "" : "Borrowings"),
            new KeyValuePair<BizType, string>(BizType.DebtRepayment, _useCaps ? "" : "Debt Repayment"),
            new KeyValuePair<BizType, string>(BizType.LoansRepaid, _useCaps ? "" : "Loans Repaid"),
            new KeyValuePair<BizType, string>(BizType.Lendings, _useCaps ? "" : "Lendings"),
            new KeyValuePair<BizType, string>(BizType.PoolTransactions, _useCaps ? "" : "Pool transactions"),
            new KeyValuePair<BizType, string>(BizType.InstantExchange, _useCaps ? "" : "Instant Exchange"),
            new KeyValuePair<BizType, string>(BizType.SubAccountTransfer, _useCaps ? "SUB_TRANSFER" : "Sub-account transfer"),
            new KeyValuePair<BizType, string>(BizType.LiquidationFees, _useCaps ? "" : "Liquidation Fees"),
            new KeyValuePair<BizType, string>(BizType.VotingEarnings, _useCaps ? "" : "Voting Earnings"),
            new KeyValuePair<BizType, string>(BizType.RedemptionOfVoting, _useCaps ? "" : "Redemption of Voting"),
            new KeyValuePair<BizType, string>(BizType.Voting, _useCaps ? "" : "Voting"),

            new KeyValuePair<BizType, string>(BizType.CrossMargin, _useCaps ? "" : "Cross Margin"),
            new KeyValuePair<BizType, string>(BizType.MiningIncome, _useCaps ? "" : "Mining Income"),
            new KeyValuePair<BizType, string>(BizType.BankCardDeal, _useCaps ? "" : "Bank Card Deal"),
            new KeyValuePair<BizType, string>(BizType.MarginBonus, _useCaps ? "" : "Bonus received"),
            new KeyValuePair<BizType, string>(BizType.LiquidationTakeover, _useCaps ? "" : "Liquidation Takeover"),
            new KeyValuePair<BizType, string>(BizType.ReturnOfLiquidationTakeover, _useCaps ? "" : "Return of Liquidation Takeover"),
        };
    }
}
