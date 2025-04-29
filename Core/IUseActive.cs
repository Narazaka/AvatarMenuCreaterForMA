namespace net.narazaka.avatarmenucreator
{
    public interface IUseActive
    {
        bool UseActive { get; set; }
        bool UseInactive { get; set; }
        bool UseTransitionToActive { get; set; }
        bool UseTransitionToInactive { get; set; }
    }
}
