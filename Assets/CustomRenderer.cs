using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Execute in edit mode so we don't have to enter play mode to see the results
[ExecuteInEditMode]
public class CustomRenderer : MonoBehaviour {

    public Mesh m_Mesh;
    public Material m_Material;
    public Light m_Light;

    CommandBuffer m_CommandBuffer;
    CommandBuffer m_LightCommandBuffer;

    HashSet<Camera> m_Cameras;

    LightEvent m_LightEvent = LightEvent.BeforeShadowMapPass; // We render the mesh into the shadow map for each pass (for each cascade for example)
    CameraEvent m_CameraEvent = CameraEvent.BeforeGBuffer; // Render mesh into GBuffer (so, this example only works for deferred rendering I'm afraid)

    void OnEnable()
    {
        m_Cameras = new HashSet<Camera>();

        m_CommandBuffer = new CommandBuffer();
        m_CommandBuffer.name = "Custom Renderer";

        m_LightCommandBuffer = new CommandBuffer();
        m_LightCommandBuffer.name = "Custom Renderer Shadows";

        UpdateCommandBuffers();

        m_Light.AddCommandBuffer(m_LightEvent, m_LightCommandBuffer); // Could also specify which shadow pass to add the CommandBuffer to here e.g. ShadowMapPass.DirectionalCascade0
    }

    void OnDisable()
    {
        if (m_Cameras != null)
        {
            foreach (Camera cam in m_Cameras)
            {
                if (cam != null)
                    cam.RemoveCommandBuffer(m_CameraEvent, m_CommandBuffer);
            }

            m_Cameras.Clear();
            m_Cameras = null;
        }

        if (m_Light != null)
            m_Light.RemoveCommandBuffer(m_LightEvent, m_LightCommandBuffer);
    }

    void UpdateCommandBuffers()
    {
        if (m_CommandBuffer != null)
        {
            m_CommandBuffer.Clear();
            int passIndex = m_Material.FindPass("MAINPASS");
            m_CommandBuffer.DrawMesh(m_Mesh, transform.localToWorldMatrix, m_Material, 0, passIndex);
        }

        if (m_LightCommandBuffer != null)
        {
            m_LightCommandBuffer.Clear();
            int passIndex = m_Material.FindPass("SHADOWPASS");
            m_LightCommandBuffer.DrawMesh(m_Mesh, transform.localToWorldMatrix, m_Material, 0, passIndex);
        }
    }

    // A MeshRenderer needs to be attached to this GameObject in order for this function (OnWillRenderObject) to get called by Unity
    void OnWillRenderObject()
    {
        Camera cam = Camera.current;

        if (cam == null)
            return;

        // Update CommandBuffers as the matrix may have changed
        UpdateCommandBuffers();

        if (!m_Cameras.Contains(cam))
        {
            cam.AddCommandBuffer(m_CameraEvent, m_CommandBuffer);
            m_Cameras.Add(cam);
        }
    }
}
