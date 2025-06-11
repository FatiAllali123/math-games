namespace com.glups.Reward
{
    public class FixedIntervalSchedule : RewardStrategy
    {

        public FixedIntervalSchedule(RewardModel model, StrategyParameters p) : base(model, p)
        {
        }

        internal override string name()
        {
            return "FIS";
        }
        internal override int addPositive(int points)
        {
            return _model.addPositive(points, points);
        }

        internal override int addNegative(int errors)
        {

            // We will add some points as incentive to the reward
            // we compute the incentive here.
            float incentive = (errors * _parameters.theFixedIncentiveRatio);

            return _model.addNegative(errors, incentive);
        }

        internal override int updateReward()
        {
            return _model.updateReward(_parameters.theFixedSchedule);
        }
    }

}
