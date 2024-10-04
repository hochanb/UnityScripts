using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;


namespace ShootingStaa.SceneViewAlign
{


    public class CustomSceneView : EditorWindow
    {
        private static AlignMode currentAlignMode = AlignMode.None;

        // Define alignment modes
        private enum AlignMode
        {
            None,
            X,
            Y,
            Z
        }

        // Set alignment mode and update the scene view
        static void SetAlignMode(AlignMode mode)
        {
            if (mode == AlignMode.None)
            {
                SceneView.duringSceneGui -= OnSceneGUI; // Remove listener for OnSceneGUI
            }
            else
            {
                SceneView.duringSceneGui -= OnSceneGUI; // Remove listener for OnSceneGUI
                SceneView.duringSceneGui += OnSceneGUI; // Add listener for OnSceneGUI
            }

            currentAlignMode = mode;
            SceneView.lastActiveSceneView.Repaint();
        }

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            SceneView.duringSceneGui += DrawToggle;
        }

        static void OnSceneGUI(SceneView sceneView)
        {


            Vector3 cameraPosition = sceneView.camera.transform.position;
            Vector3 cameraForward = sceneView.camera.transform.forward;
            Vector3 cameraUp = -cameraPosition;
            Quaternion cameraRotation;

            Vector3 camPos = sceneView.pivot - sceneView.rotation * Vector3.forward * sceneView.cameraDistance;

            switch (currentAlignMode)
            {
                case AlignMode.None:
                    cameraUp = Vector3.up; 
                    break;

                case AlignMode.X:
                    cameraUp.x = 0;
                    break;
                case AlignMode.Y:
                    cameraUp.y = 0;
                    break;
                case AlignMode.Z:
                    cameraUp.z = 0;

                    break;
            }

            cameraUp = cameraUp.normalized;
            Vector3 cameraRight = Vector3.Cross(cameraForward, cameraUp);
            cameraRotation = Quaternion.LookRotation(cameraForward, cameraUp);


            Event e = Event.current;
            if (e.type == EventType.MouseDrag && e.button == 1) // Right mouse drag
            {
                Vector2 mouseDelta = e.delta;

                // Sensitivity for mouse movement (adjust as needed)
                float sensitivity = 0.2f;

                // Rotate around the custom up vector (yaw)
                Quaternion yawRotation = Quaternion.AngleAxis(mouseDelta.x * sensitivity, cameraUp);

                // Rotate around the local right vector (pitch)
                Quaternion pitchRotation = Quaternion.AngleAxis(-mouseDelta.y * sensitivity, cameraRight);

                // Apply the combined rotation (yaw then pitch)
                cameraRotation = yawRotation * pitchRotation * cameraRotation;

                // Consume the event so Unity doesn't handle it in default way
                e.Use();
            }



            sceneView.rotation = cameraRotation;
            sceneView.pivot = camPos + sceneView.rotation * Vector3.forward * sceneView.cameraDistance;

            // Repaint the scene view to reflect changes
            sceneView.Repaint();
        }


        static void DrawToggle(SceneView view)
        {
            const float buttonWidth = 30; // button width
            const float buttonHeight = 14; // button height
            float buttonYPosition = 120; // Y position of the buttons

            // Calculate the position for the buttons
            Vector2 buttonPositionX = new Vector2(view.position.width - 100, buttonYPosition);
            Vector2 buttonPositionY = new Vector2(view.position.width - 70, buttonYPosition);
            Vector2 buttonPositionZ = new Vector2(view.position.width - 40, buttonYPosition);

            Handles.BeginGUI();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                GUI.color = Color.white; 
                // Draw the toggle for the X alignment
                bool useX = currentAlignMode == AlignMode.X; // Check if current mode is X
                bool useX_new = GUI.Toggle(new Rect(buttonPositionX.x, buttonPositionX.y, buttonWidth, buttonHeight), useX, "X", EditorStyles.miniButtonLeft);
                bool setX = !useX && useX_new;

                // Draw the toggle for the Y alignment
                bool useY = currentAlignMode == AlignMode.Y; // Check if current mode is Y
                bool useY_new = GUI.Toggle(new Rect(buttonPositionY.x, buttonPositionY.y, buttonWidth, buttonHeight), useY, "Y", EditorStyles.miniButtonMid);
                bool setY = !useY && useY_new;

                // Draw the toggle for the Z alignment
                bool useZ = currentAlignMode == AlignMode.Z; // Check if current mode is Z
                bool useZ_new = GUI.Toggle(new Rect(buttonPositionZ.x, buttonPositionZ.y, buttonWidth, buttonHeight), useZ, "Z", EditorStyles.miniButtonRight);
                bool setZ = !useZ && useZ_new;

                // Handle button changes
                if (changeCheck.changed)
                {
                    // Set the alignment mode based on which button was toggled
                    if (setX)
                        SetAlignMode(AlignMode.X);
                    else if (setY)
                        SetAlignMode(AlignMode.Y);
                    else if (setZ)
                        SetAlignMode(AlignMode.Z);
                    else if(!useX_new && !useY_new && !useZ_new)
                        SetAlignMode(AlignMode.None); // If none are selected, set to None
                }
            }

            Handles.EndGUI();
        }
    }
}
