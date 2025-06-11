namespace com.glups.Reward
{
    public class VariableRatioSchedule : RewardStrategy
    {
        // the static values need to be defined
        // These values can be adapted to the game. In our case, our mean value is 10.

        private int _variableThreshold = 0;

        public VariableRatioSchedule(RewardModel model, StrategyParameters p) : base(model, p)
        {
        }

        internal override string name()
        {
            return "VRS";
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
            // computes the next Threshold with some random values (Variable)
            if (_variableThreshold == 0)
            {
                _variableThreshold = _parameters.computeVariableSchedule();
            }


            int rewards = _model.updateReward(_variableThreshold);

            if (rewards > 0)
            {
                // ask to update the threshold in the next call 
                _variableThreshold = 0;
            }

            return rewards;
        }
    }
}