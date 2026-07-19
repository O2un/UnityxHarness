using System;
using System.Collections.Generic;
using O2un.Manager;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class UpgradeCardAcquisition : IDisposable
    {
        public const int MAX_UPGRADE_SLOT = 6;

        private const int CANDIDATE_COUNT = 2;

        private readonly UpgradeCardPoolSO _pool;
        private readonly IInventoryReader _reader;
        private readonly IInventoryWriter _writer;

        private readonly List<UpgradeCardSO> _drawBuffer = new();

        private readonly Subject<IReadOnlyList<UpgradeCardSO>> _onCandidatesDrawn = new();
        private readonly Subject<IUpgradeCardData> _onCardAcquired = new();
        private readonly Subject<Unit> _onSlotsFull = new();

        public Observable<IReadOnlyList<UpgradeCardSO>> OnCandidatesDrawn => _onCandidatesDrawn;
        public Observable<IUpgradeCardData> OnCardAcquired => _onCardAcquired;
        public Observable<Unit> OnSlotsFull => _onSlotsFull;

        public int AcquiredCount => CountAcquired();

        public UpgradeCardAcquisition(UpgradeCardPoolSO pool, IInventoryReader reader, IInventoryWriter writer)
        {
            _pool = pool;
            _reader = reader;
            _writer = writer;
        }

        public IReadOnlyList<UpgradeCardSO> DrawCandidates()
        {
            _drawBuffer.Clear();

            IReadOnlyList<UpgradeCardSO> cards = _pool.Cards;

            var remaining = new List<UpgradeCardSO>();
            for (int i = 0; null != cards && i < cards.Count; i++)
            {
                UpgradeCardSO card = cards[i];
                if (null == card)
                {
                    continue;
                }

                // 패시브는 해금 집합이라 두 번째 장이 슬롯만 쓰고 무효가 된다. 추첨에서 아예 뺀다.
                if (UpgradeCardKind.PassiveSkill == card.Kind && true == IsPassiveOwned(card.PassiveSkill))
                {
                    continue;
                }

                remaining.Add(card);
            }

            if (remaining.Count < CANDIDATE_COUNT)
            {
                Debug.LogWarning($"[UpgradeCardAcquisition] 뽑을 수 있는 카드가 {CANDIDATE_COUNT}장 미만입니다 — 뽑은 만큼만 반환합니다.");
            }

            int drawCount = Mathf.Min(CANDIDATE_COUNT, remaining.Count);
            for (int i = 0; i < drawCount; i++)
            {
                int index = UnityEngine.Random.Range(0, remaining.Count);
                _drawBuffer.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            _onCandidatesDrawn.OnNext(_drawBuffer);
            return _drawBuffer;
        }

        public bool TryAcquire(IUpgradeCardData card)
        {
            if (null == card)
            {
                return false;
            }

            if (CountAcquired() >= MAX_UPGRADE_SLOT)
            {
                _onSlotsFull.OnNext(Unit.Default);
                return false;
            }

            AddResult result = _writer.Add(card);
            if (AddResult.SlotsFull == result)
            {
                _onSlotsFull.OnNext(Unit.Default);
                return false;
            }

            _onCardAcquired.OnNext(card);
            return true;
        }

        public void Dispose()
        {
            _onCandidatesDrawn.Dispose();
            _onCardAcquired.Dispose();
            _onSlotsFull.Dispose();
        }

        private bool IsPassiveOwned(PassiveSkillType skill)
        {
            IReadOnlyList<InventorySlot> slots = _reader.Slots.CurrentValue;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item is not IUpgradeCardData card)
                {
                    continue;
                }

                if (UpgradeCardKind.PassiveSkill == card.Kind && skill == card.PassiveSkill)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountAcquired()
        {
            IReadOnlyList<InventorySlot> slots = _reader.Slots.CurrentValue;
            int count = 0;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item is IUpgradeCardData)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
