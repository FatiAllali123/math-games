namespace com.glups.Reward
{
    public class VariableIntervalSchedule : VariableRatioSchedule
    {

        public VariableIntervalSchedule(RewardModel model, StrategyParameters p) : base(model, p)
        {
        }

        internal override string name()
        {
            return "VIS";
        }
        internal override int addNegative(int errors)
        {

            // We will add some points as incentive to the iScore governing the reward

            return _model.addNegative(errors, _parameters.computeVariableIncentive(errors));
        }

    }

}
