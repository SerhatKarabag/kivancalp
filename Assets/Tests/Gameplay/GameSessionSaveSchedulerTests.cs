using System;
using Kivancalp.Gameplay.Application;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class GameSessionSaveSchedulerTests
    {
        [Test]
        public void MarkDirty_Tick_SavesAfterDebounce()
        {
            int saveCount = 0;
            var scheduler = new GameSessionSaveScheduler(saveDebounceSeconds: 1.0f);

            scheduler.MarkDirty(() => { saveCount++; return true; });

            scheduler.Tick(0.5f, () => { saveCount++; return true; });
            Assert.AreEqual(0, saveCount, "Should not save before debounce");

            scheduler.Tick(0.5f, () => { saveCount++; return true; });
            Assert.AreEqual(1, saveCount, "Should save after debounce elapsed");
        }

        [Test]
        public void MarkDirty_ZeroDebounce_SavesImmediately()
        {
            int saveCount = 0;
            var scheduler = new GameSessionSaveScheduler(saveDebounceSeconds: 0f);

            scheduler.MarkDirty(() => { saveCount++; return true; });

            Assert.AreEqual(1, saveCount);
        }

        [Test]
        public void Tick_NotDirty_DoesNotSave()
        {
            int saveCount = 0;
            var scheduler = new GameSessionSaveScheduler(saveDebounceSeconds: 0f);

            scheduler.Tick(1.0f, () => { saveCount++; return true; });

            Assert.AreEqual(0, saveCount);
        }

        [Test]
        public void ForceSave_BypassesDebounce()
        {
            int saveCount = 0;
            var scheduler = new GameSessionSaveScheduler(saveDebounceSeconds: 10.0f);

            scheduler.ForceSave(() => { saveCount++; return true; });

            Assert.AreEqual(1, saveCount);
        }

        [Test]
        public void Reset_ClearsDirtyFlag()
        {
            int saveCount = 0;
            var scheduler = new GameSessionSaveScheduler(saveDebounceSeconds: 1.0f);

            scheduler.MarkDirty(() => { saveCount++; return true; });
            scheduler.Reset();

            scheduler.Tick(2.0f, () => { saveCount++; return true; });

            Assert.AreEqual(0, saveCount, "Should not save after reset");
        }

        [Test]
        public void SaveFails_RemainsDirty_RetriesOnNextTick()
        {
            int saveAttempts = 0;
            var scheduler = new GameSessionSaveScheduler(saveDebounceSeconds: 0.5f);

            scheduler.MarkDirty(() =>
            {
                saveAttempts++;
                return false;
            });

            scheduler.Tick(1.0f, () =>
            {
                saveAttempts++;
                return false;
            });
            Assert.AreEqual(1, saveAttempts, "First save attempt");

            scheduler.Tick(1.0f, () =>
            {
                saveAttempts++;
                return true;
            });
            Assert.AreEqual(2, saveAttempts, "Should retry because previous save failed");
        }

        [Test]
        public void ForceSave_NullOperation_ThrowsArgumentNullException()
        {
            var scheduler = new GameSessionSaveScheduler(saveDebounceSeconds: 0f);

            Assert.Throws<ArgumentNullException>(() => scheduler.ForceSave(null));
        }
    }
}
