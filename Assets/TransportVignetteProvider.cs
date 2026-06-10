using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Simple vignette provider you can trigger manually for "transport" effects.
    /// </summary>
    public class TransportVignetteProvider : MonoBehaviour, UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.ITunnelingVignetteProvider
    {
        [SerializeField]
        UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.VignetteParameters m_Parameters = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.VignetteParameters();

        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.VignetteParameters vignetteParameters => m_Parameters;
 
        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.VignetteParameters parameters => m_Parameters; // handy accessor
    }
}
