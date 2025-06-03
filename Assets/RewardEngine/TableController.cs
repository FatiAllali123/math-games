using UnityEngine;
namespace com.glups.Reward
{

    public class TableController : Controller
    {
        // This controller adds the management of a score local to tables.
        // When the score of the table become > than oldScore for this table, this controller start propagating the additional points to the model, calling addPositive.
        // Scene should store that score with a table.
        // Scene creates this controller each time it starts, and calls openTable(10, previousScore).
        // When score > oldMaxScore, then
        internal int _maxScore = 0;
        internal int _oldMaxScore = 0;
        internal int _tScore = 0;

        public TableController(RewardModel model, View view) : base(model, view) {}

        internal virtual void openTable(int maxScore, int previousMaxScore)
        {

            if (maxScore < previousMaxScore)
            {
                Debug.Log("TableController: previous max score must be lower than max score possible: " + maxScore + ", " + previousMaxScore);
                previousMaxScore = maxScore;
            }

            _maxScore = maxScore;
            _oldMaxScore = previousMaxScore;
            _tScore = 0;
        }

        internal virtual int closeTable()
        {
            // returns the new max score of that table.
            // the caller should store this max score

            if (_oldMaxScore < _tScore)
            {
                _oldMaxScore = _tScore;
            }
            return _oldMaxScore;
        }

        internal override void addPositive(int nsuccess)
        {
            // adds nsuccess to the local table score called tScore.
            // If tScore become greater than the oldMaxScore, then we start propagating the nsuccess to the model calling super.addPositive.
            // This methods checks also that the total score stays below the maxStore, given initially.

            if (_tScore >= _maxScore)
            {
                return;
            }

            if (_tScore > _oldMaxScore)
            {
                if (_tScore + nsuccess >= _maxScore)
                {
                    nsuccess = _maxScore - _tScore;
                }
                _tScore += nsuccess;
                base.addPositive(nsuccess);
                return;
            }

            if (_tScore + nsuccess > _oldMaxScore)
            {
                int score2propagate = nsuccess - (_oldMaxScore - _tScore);
                _tScore += nsuccess;
                base.addPositive(score2propagate);
            }
            else
            {
                _tScore += nsuccess;
            }
        }
    }
}