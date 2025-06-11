// the test controller of our library
using UnityEngine;
namespace com.glups.Reward
{
    public class Controller
    {

        internal RewardModel _model;
        internal RewardStrategy _FRS, _VRS, _VIS, _FIS;
        internal RewardStrategy _currentStrategy;
        internal View _view;

        public Controller(RewardModel model, View view)
        {
            _model = model;
            StrategyParameters parameters = new StrategyParameters();
            _FRS = new FixedRatioSchedule(_model, parameters);
            _FIS = new FixedIntervalSchedule(_model, parameters);
            _VRS = new VariableRatioSchedule(_model, parameters);
            _VIS = new VariableIntervalSchedule(_model, parameters);

            _view = view;
            _currentStrategy = null;
        }

        internal virtual void openLevel(int level)
        {
            _model.openLevel(level);

            _currentStrategy = _FIS;
        }

        internal virtual void traceModel(int date, int playerID)
        {
            string strategyName = "?";

            if (_currentStrategy != null)
            {
                strategyName = _currentStrategy.name();
            }
            _model.trace(date, playerID, strategyName);
        }

        internal virtual void addPositive(int nsuccess)
        {
            if (_currentStrategy == null)
            {
                Debug.Log("Controller: must select a strategy before calling addPositive");
                return;
            }
            _currentStrategy.addPositive(nsuccess); // this returns the iScore
                                                    //_view.updatePositiveScore(_model.getScore(), _model.getRewardScore());

            int rank = _currentStrategy.updateRank();
            if (rank > 0)
            {
                _view.updateRank(rank);
            }

            int reward = _currentStrategy.updateReward();
            if (reward > 0)
            {
                _view.updateReward(reward);
            }
        }

        internal virtual void addNegative(int nfailure)
        {
            if (_currentStrategy == null)
            {
                Debug.Log("Controller: must select a strategy before calling addNegative");
                return;
            }
            _currentStrategy.addNegative(nfailure); // this returns the iScore
                                                    //_view.updateNegativeScore(_model.getScore(), _model.getRewardScore());

            int rank = _currentStrategy.updateRank();
            if (rank > 0)
            {
                _view.updateRank(rank);
            }

            int reward = _currentStrategy.updateReward();
            if (reward > 0)
            {
                _view.updateReward(reward);
            }
        }
    }
}