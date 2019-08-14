using System.Collections.Generic;
using UnityEngine;

namespace PofyTools.Distribution
{
    public interface IShufflable
    {
        bool IsShuffled
        {
            get;
        }

        void Shuffle();
    }

    /// <summary>
    /// Generic Deck. Deck contains cards that contain references to the instances of the teplated type.
    /// </summary>
    [System.Serializable]
    public class Deck<T> : IShufflable
    {
        [System.Serializable]
        public class Card
        {
            [SerializeField] protected T _element = default(T);

            public T Element
            {
                get { return this._element; }
            }

            [SerializeField] protected int _weight = 0;

            public int Weight
            {
                get { return this._weight; }
                set { this._weight = Mathf.Max(1, value); }
            }

            public override string ToString()
            {
                return string.Format("[Card: instance={0}, weight={1}]", this._element, this._weight);
            }

            public Card(T element, int weight = 1)
            {
                this._element = element;
                this._weight = weight;
            }

            public Card(Card card)
            {
                this._element = card.Element;
                this._weight = card.Weight;
            }
        }

        #region State

        public enum State : int
        {
            Empty = 0,
            Initialized = 1,
            Populated = 2,
            Shuffled = 3,
            Distributed = 4,
        }

        [SerializeField] protected State _state = State.Empty;

        public State CurrentState { get { return this._state; } }

        #endregion

        #region IShufflable

        //protected bool _isShuffled = false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="PofyTools.Deck`1"/> is shuffled.
        /// </summary>
        /// <value><c>true</c> if is shuffled; otherwise, <c>false</c>.</value>
        public bool IsShuffled
        {
            get { return this._state == State.Shuffled; }
        }

        /// <summary>
        /// Shuffles this Deck.
        /// </summary>
        public void Shuffle()
        {
            this._head = 0;

            while (this._head < this.Count)
            {
                int randomIndex = Random.Range(this._head, this.Count);
                Card randomCard = this._cards[randomIndex];
                this._cards.RemoveAt(randomIndex);
                this._cards.Insert(this._head, randomCard);

                ++this._head;
            }

            this._state = State.Shuffled;

            this._head = 0;
        }

        #endregion

        [SerializeField] protected List<Card> _cards = new List<Card>();

        public List<Card> Cards
        {
            get { return this._cards; }
        }

        [SerializeField] protected bool _autoShuffle = true;
        public bool AutoShuffle => this._autoShuffle;

        public bool IsDepleted => this._head == this._cards.Count;

        [SerializeField] protected int _head;

        /// <summary>
        /// Gets the currenct position in the deck or -1 if the Deck is empty.
        /// </summary>
        /// <value>The head.</value>
        public int Head
        {
            get
            {
                if (this.Count > 0)
                    return this._head;
                else
                    return -1;
            }
        }

        /// <summary>
        /// Gets the total count of<see cref="PofyTools.Deck.Card"/>in the Deck.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return this._cards.Count; }
        }

        [SerializeField] protected int _maxWeight = int.MinValue;

        /// <summary>
        /// Returns cached max weight or gets the max weight present in the Deck or <c>int.MinValue</c> if empty.
        /// </summary>
        /// <value>The max weight.</value>
        public int MaxWeight
        {
            get
            {
                if (this._maxWeight == int.MinValue)
                {
                    this._maxWeight = GetMaxWeight();
                }

                return this._maxWeight;
            }
        }

        protected int GetMaxWeight()
        {
            int result = int.MinValue;
            for (int i = 0, max_cardsCount = this._cards.Count; i < max_cardsCount; i++)
            {
                var card = this._cards[i];
                result = Mathf.Max(result, card.Weight);
            }

            return result;
        }

        [SerializeField] protected int _minWeight = int.MaxValue;

        /// <summary>
        /// Returns cached min weight or gets the min weight present in the Deck or <c>int.MaxValue</c> if empty.
        /// </summary>
        /// <value>The min weight.</value>
        public int MinWeight
        {
            get
            {
                if (this._minWeight == int.MaxValue)
                {
                    this._minWeight = GetMinWeight();
                }

                return this._minWeight;
            }
        }

        protected int GetMinWeight()
        {
            int result = int.MaxValue;
            for (int i = 0, max_cardsCount = this._cards.Count; i < max_cardsCount; i++)
            {
                var card = this._cards[i];
                result = Mathf.Min(result, card.Weight);
            }

            return result;
        }

        /// <summary>
        /// Returns whether the <see cref="PofyTools.Deck.Card"/>instance is present in the Deck.
        /// </summary>
        /// <returns><c>true</c>, if the Card instance is present in the Deck, <c>false</c> otherwise.</returns>
        /// <param name="card">Card.</param>
        public bool ContainsCard(Card card)
        {
            return this._cards.Contains(card);
        }

        /// <summary>
        /// Adds the card to the Deck. Sets the Deck's Max Weight to the Card's weight if greater than current Max Weight.
        /// </summary>
        /// <param name="card">Card.</param>
        public void AddCard(Card card)
        {
            this._cards.Add(card);
            if (card.Weight > this.MaxWeight)
            {
                this._maxWeight = card.Weight;
            }
            else if (card.Weight < this.MinWeight)
            {
                this._minWeight = card.Weight;
            }
        }

        /// <summary>
        /// Creates and adds the card for the element provided with the weight provided (default 1).
        /// </summary>
        /// <param name="element">Instance.</param>
        /// <param name="weight">Weight.</param>
        public Card AddElement(T element, int weight = 1)
        {
            Card card = new Card(element, weight);
            AddCard(card);
            return card;
        }

        public void AddIdentityElement(T element, int weight = 1)
        {
            Card identityCard = AddElement(element, weight);

            SetIdentityCard(identityCard);
        }

        public void RemoveCard(Card card)
        {
            this._cards.RemoveAll(c => c == card);
            this._maxWeight = GetMaxWeight();
            this._minWeight = GetMinWeight();
            //TODO: collect extremes in one iteration
        }

        public void RemoveElementCard(T element)
        {
            this._cards.RemoveAll(c => (object)c.Element == (object)element);
            this._maxWeight = GetMaxWeight();
            this._minWeight = GetMinWeight();
            //TODO: collect extremes in one iteration
        }

        /// <summary>
        /// Returns whether the Deck contains the Card with the provided elemenet.
        /// </summary>
        /// <returns><c>true</c>, if instance was containsed, <c>false</c> otherwise.</returns>
        /// <param name="element">Instance.</param>
        public bool ContainsElement(T element)
        {
            for (int i = 0, max_cardsCount = this._cards.Count; i < max_cardsCount; i++)
            {
                var card = this._cards[i];
                if ((object)card.Element == (object)element)
                    return true;
            }
            return false;
        }

        public bool ContainsElement(T element, out Card elementCard)
        {
            for (int i = 0, max_cardsCount = this._cards.Count; i < max_cardsCount; i++)
            {
                var card = this._cards[i];
                if ((object)card.Element == (object)element)
                {
                    elementCard = card;
                    return true;
                }
            }
            elementCard = null;
            return false;
        }

        public Card FindElementCard(T element)
        {
            Card card = null;
            if (ContainsElement(element, out card))
            {
                Debug.Log("Instance card found!");
                return card;
            }
            Debug.Log("Instance card not found!");
            return card;
        }

        public Card PickElementCard(T element)
        {
            Card instanceCard = null;
            for (int i = this._head, max_cardsCount = this._cards.Count; i < max_cardsCount; i++)
            {
                var card = this._cards[i];
                if ((object)card.Element == (object)element)
                {
                    instanceCard = PickCardAt(i);
                    break;
                }
            }
            return instanceCard;
        }

        protected Card PickCardAt(int index, bool reorder = true)
        {
            if (!this.IsDepleted)
            {
                Card resultCard = this._cards[index];

                if (index != 0 && reorder)
                {
                    this._cards.RemoveAt(index);
                    this._cards.Insert(0, resultCard);
                }

                ++this._head;
                if (this.IsDepleted && this._autoShuffle)
                {
                    Shuffle();
                }

                return resultCard;
            }

            Debug.LogWarning("Deck is depleted. Returning null...");
            return null;

        }

        /// <summary>
        /// Picks the Card on the Head position and moves the Head to next position.
        /// If Head gets to the end of the Deck, the Deck gets reshuffled. 
        /// </summary>
        /// <returns>The next card.</returns>
        public Card PickNextCard()
        {
            return PickCardAt(this._head, false);
        }

        /// <summary>
        /// Picks the first card with minWeight or higher and removes it from the
        /// </summary>
        /// <returns>The bias card.</returns>
        /// <param name="minWeight">Minimum weight.</param>
        public Card PickBiasCard(int minWeight = 0)
        {
            minWeight = Mathf.Min(minWeight, this.MaxWeight);

            Card biasCard = null;
            for (int i = this._head, max_cardsCount = this._cards.Count; i < max_cardsCount; ++i)
            {
                var card = this._cards[i];
                if (card.Weight >= minWeight)
                {
                    biasCard = PickCardAt(i);
                    break;
                }
            }

            return biasCard;
        }

        //TODO: different distribution types
        public Deck<T> CreateDistributionDeck()
        {
            Deck<T> distributionDeck = null;
            if (this._state != State.Distributed)
            {

                distributionDeck = new Deck<T>();

                foreach (var card in this._cards)
                {
                    card.Weight = Mathf.Max(1, card.Weight);
                    //				int totalNumberOfCopies = Mathf.RoundToInt ((float)this.MaxWeight / (float)card.weight);
                    int count = card.Weight;
                    //				AlertCanvas.Instance.alert (string.Format ("{0} : {1}", card.instance, totalNumberOfCopies), AlertPanel.Type.INFO);
                    while (count > 0)
                    {
                        Card copy = new Card(card);
                        distributionDeck._cards.Add(copy);
                        --count;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Source deck is already distributed. Returning the shallow copy of the source...");
                distributionDeck = new Deck<T>(this);
            }

            distributionDeck.Shuffle();

            distributionDeck._state = State.Distributed;

            return distributionDeck;
        }

        #region Identity

        protected Card _identityCard = null;

        public Card IdentityCard
        {
            get { return this._identityCard; }
        }

        public bool HasIdentityCard
        {
            get { return this._identityCard != null; }
        }

        public bool IsIdentityCard(Card card)
        {
            return card == this._identityCard;
        }

        public void SetIdentityCard(Card card)
        {
            if (!this.HasIdentityCard)
                this._identityCard = card;
            else if (IsIdentityCard(card))
                Debug.LogWarningFormat("Deck: Card {0} is already identity card!", card);
            else
                Debug.LogWarningFormat("Deck: Identity card already set!");
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PofyTools.Deck`1"/> class.
        /// </summary>
        public Deck(bool autoShuffle = true)
        {
            this._cards = new List<Card>();
            this._autoShuffle = autoShuffle;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PofyTools.Deck`1"/> class.
        /// </summary>
        /// <param name="capacity">Capacity of the card list.</param>
        public Deck(int capacity, bool autoShuffle = true)
        {
            this._cards = new List<Card>(capacity);
            this._autoShuffle = autoShuffle;
        }

        public Deck(bool autoShuffle = true, params T[] elements)
        {
            this._cards = new List<Card>(elements.Length);

            for (int i = 0, instancesLength = elements.Length; i < instancesLength; i++)
            {
                var instance = elements[i];
                AddElement(instance);
            }

            this._autoShuffle = autoShuffle;
        }

        public Deck(List<Card> cards, bool autoShuffle = true)
        {
            this._cards = new List<Card>(cards);
            this._autoShuffle = autoShuffle;
        }

        public Deck(Deck<T> source)
        {
            this._cards = new List<Card>(source._cards);
            this._head = source._head;
            //this._isShuffled = source._isShuffled;
            this._state = source._state;
            this._maxWeight = source._maxWeight;
            this._minWeight = source._minWeight;
            if (source.HasIdentityCard)
                this._identityCard = FindElementCard(source._identityCard.Element);

            this._autoShuffle = source._autoShuffle;
        }

        public Deck(bool autoShuffle = true, params Card[] cards)
        {
            this._cards = new List<Card>(cards);
            this._autoShuffle = autoShuffle;
        }

        #endregion
    }
}

