using System;
using O2un.Actors;
using R3;
using VContainer.Unity;

namespace O2un.Progression
{
    public sealed class ExperienceGainContext : IInitializable, IDisposable
    {
        private readonly IExpGainedSource _expSource;
        private readonly IExperienceWriter _experienceWriter;

        private readonly CompositeDisposable _disposables = new();

        public ExperienceGainContext(IExpGainedSource expSource, IExperienceWriter experienceWriter)
        {
            _expSource = expSource;
            _experienceWriter = experienceWriter;
        }

        public void Initialize()
        {
            _expSource.OnGained.Subscribe(amount => _experienceWriter.Gain(amount)).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
