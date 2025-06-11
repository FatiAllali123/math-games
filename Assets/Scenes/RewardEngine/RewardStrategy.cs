// This class implement the reward strategy and uses RewardModel
namespace com.glups.Reward
{
    public abstract class RewardStrategy
    {
        protected internal RewardModel _model = null!;
        protected internal StrategyParameters _parameters = null!;

        private RewardStrategy()
        {
            _model = null!;
        }

        public RewardStrategy(RewardModel model, StrategyParameters parameters)
        {
            _model = model;
            _parameters = parameters;
        }

        internal abstract string name();
        internal abstract int addPositive(int points);
        internal abstract int addNegative(int points);

        internal abstract int updateReward(); //return -1 if no update is applicable, otherwise returns the value of the reward.

        internal virtual int updateRank()
        {
            // update the rank according to the _score and return always the computed rank.
            // ranks start at 1.
            // score is always higher or equal to the step associated with its rank.
            // returns 0 if rank stays unchanged.

            return _model.updateRank(_parameters.theRankSteps);
        }
    }

}
