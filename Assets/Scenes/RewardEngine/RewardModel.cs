// This class stores the model of the rewarding system
// The rewarding system is coded following the MVC design pattern.
// This is the Model part of the MVC.
using UnityEngine;
using System;
namespace com.glups.Reward
{
    public class RewardModel
    {

        internal int _currentLevel = 0;

        internal int _score = 0;
        internal int _rank = 0;

        internal int _iScore = 0;
        internal float _incentive = 0.001f;

        internal int _positives = 0;
        internal int _negatives = 0;

        internal int _reward = 0;
        internal int _previousThreshold = 0;
        internal int _nextThreshold = 0;

        // display 
        internal virtual void trace(int date, int playerID, string strategy)
        {
            Debug.Log("\t" + date + ", " + playerID + ", " + strategy + ", " + "<level " + _currentLevel + "> " + "<Score " + _score + "> <rank " + _rank + "> " + "<iScore " + _iScore + ", " + string.Format("{0:F04}", _incentive) + ", " + _nextThreshold + "> <reward " + _reward + "> " + "<positives " + _positives + "> <negatives " + _negatives + ">");
        }

        // Accessors
        internal virtual int Score
        {
            get
            {
                return _score;
            }
        }
        internal virtual int RewardScore
        {
            get
            {
                return _iScore;
            }
        }

        // Methods
        public virtual void openLevel(int level)
        {
            _currentLevel = level;
            _positives = 0;
            _negatives = 0;
            _iScore = _score;
            _incentive = 0.001f;
            _previousThreshold = _iScore;
            _nextThreshold = _iScore;
        }

        internal virtual int Positives
        {
            get
            {
                return _positives;
            }
        }
        internal virtual int Negatives
        {
            get
            {
                return _negatives;
            }
        }

        internal virtual int addPositive(int count, int points)
        {
            // returns the score used to compute rewards
            if (count > 0)
            {
                _positives += count;
                _score += points;
                _iScore += points;
            }
            return _iScore;
        }

        internal virtual int addNegative(int errors, float incentive)
        {
            // returns the score used to compute rewards
            // incentive is accumulated until a value higher than 1 is accumulated. We add this value to iScore then.
            if (errors > 0)
            {
                _negatives += errors;
            }

            _incentive += incentive;

            // this is to check equality between 2 floats.
            if ((_incentive - 1.0f) >= 0.0001f)
            {
                _iScore += (int)((long)Math.Round(Math.Floor(_incentive), MidpointRounding.AwayFromZero));
                _incentive = 0.001f;
            }
            return _iScore;
        }

        public virtual int updateRank(int[] steps)
        {
            // Return the new computed rank. If the rank stays the same, returns 0.
            // ranks start at 0 (below the ranks), first rank is 1 and finishes at rankTable.length. Example {10; 100, 200; 300, 400}.
            // rankTable: each value is the minimum value required in that position.
            // When _score becomes higher than thin minimum, increment the rank and return the new one.

            int l = steps.Length;

            if (_rank < 0)
            {
                _rank = 0;
            }
            if (_rank > l)
            {
                Debug.Log("Error: updateRank - _rank > vector length:" + _rank + " > " + l);
                return 0;
            }

            // Score is higher then the last step of the vector.
            if (_score >= steps[l - 1])
            {
                if (_rank == l)
                {
                    return 0;
                }
                else
                {
                    return _rank = l;
                }
            }


            int oldRank = _rank;

            for (int i = _rank; i < l && steps[i] <= _score; i++)
            {
                _rank++;
            }

            // correct the _rank if it is higher then what is should be.
            while (_rank > 0 && _score < steps[_rank - 1])
            {
                _rank--;
            }

            if (_rank == oldRank)
            {
                return 0;
            }
            else
            {
                return _rank;
            }
        }

        internal virtual int distance2Reward(int points)
        {
            // check if the additional points can trigger a reward
            // returns > 0 if the reward is triggered; and returns the number of rewards above threshold
            // return <0 indicating number of values needed to get a reward

            if (_iScore + points < _nextThreshold)
            {
                return _iScore + points - _nextThreshold;
            }
            else
            {
                return _iScore + points - _nextThreshold + 1;
            }
        }


        internal virtual int updateReward(int gap)
        {
            // Return 0 if no update applicable, otherwise the current counter of rewards. 
            // update the reward counter. Side effect, update the nextThreshold

            if (gap == 0)
            {
                Debug.Log("Error: updateReward - gap == 0");
                return 0;
            }

            if (_nextThreshold == _previousThreshold)
            {
                _nextThreshold = _previousThreshold + gap;
            }

            int distance = distance2Reward(0);

            if (distance > 0)
            {
                _previousThreshold = _nextThreshold;
                _nextThreshold += gap;

                /* This is another alternative. In this case, just 1 reward is awarded even if the score is way after the threshold.
                 * do {
                 * 		_nextThreshold += gap;
                 * } while(_nextThreshold <= _iScore);
                 */

                _reward++;
                return _reward;
            }
            else
            {
                return 0;
            }

        }


    }

}
