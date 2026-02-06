using Kivancalp.Gameplay.Contracts;

namespace Kivancalp.UI.Presentation
{
    public sealed class GamePresenter
    {
        private readonly IGameSession _session;
        private readonly BoardPresenter _boardPresenter;
        private readonly HudPresenter _hudPresenter;

        public GamePresenter(IGameSession session, BoardPresenter boardPresenter, HudPresenter hudPresenter)
        {
            _session = session;
            _boardPresenter = boardPresenter;
            _hudPresenter = hudPresenter;
        }

        public void Initialize()
        {
            _boardPresenter.Initialize();
            _hudPresenter.Initialize();
            _session.Start();
        }
    }
}
