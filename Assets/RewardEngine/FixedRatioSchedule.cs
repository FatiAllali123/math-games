namespace com.glups.Reward
{
    // This class implements a Fixed Ratio Schedule Strategy

    public class FixedRatioSchedule : RewardStrategy
    {

        public FixedRatioSchedule(RewardModel model, StrategyParameters parameters) : base(model, parameters)
        {
        }

        internal override string name()
        {
            return "FRS";
        }
        internal override int addPositive(int points)
        {
            return _model.addPositive(points, points);
        }
        internal override int addNegative(int points)
        {
            return _model.addNegative(points, 0);
        }

        internal override int updateReward()
        {
            return _model.updateReward(_parameters.theFixedSchedule);
        }
    }

}
