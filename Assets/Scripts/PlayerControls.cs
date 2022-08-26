// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""GameBoard"",
            ""id"": ""8b35c3e0-ce5c-4a08-ade7-3cd5c27905e9"",
            ""actions"": [
                {
                    ""name"": ""Select"",
                    ""type"": ""Button"",
                    ""id"": ""12d05c5d-5578-478e-849b-a104b3ddd67b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MoveHover"",
                    ""type"": ""Value"",
                    ""id"": ""ae667d5a-da7f-4611-a392-e84e8396df8b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": ""Tap(duration=0.05)""
                },
                {
                    ""name"": ""DeselectAll"",
                    ""type"": ""Button"",
                    ""id"": ""fb343437-2491-477d-857a-1a83736e2168"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PauseMenu"",
                    ""type"": ""Button"",
                    ""id"": ""b979d7ea-7585-4d79-b04b-1be445bcb12a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4d8794de-f62e-4724-82b2-db3836defbc8"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c743270b-e259-47e0-8050-5b3a4171fbb0"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2c2b7524-8535-4b60-9b51-5a9239424f89"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""1cbc23b8-2d8e-42a2-8fca-cd81e38a3de3"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""daf412b4-a5c0-4c96-94ab-cb1bd7a530f9"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""c7c638bb-b00b-483b-b8b5-18901e9e8a02"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""a11c46e4-386a-49f0-82ea-c13c8c0b995b"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""9df3c3c4-1002-4fc9-b505-c0d6fc9b6582"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrow Keys"",
                    ""id"": ""18de87b3-fc57-466e-8cea-f81631eed5ff"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e156af2b-0f1b-424c-b833-589df5932fc6"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""f8621b87-360a-40bd-b0dc-57cc5d3a6d7b"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""9f8423a6-3625-4898-8e01-045ff5237f1d"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""79e05818-e7a6-4e58-8c86-89ad07703175"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""a32612e3-4538-4232-b3e0-a7920c203536"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Dpad"",
                    ""id"": ""eddb3230-94fc-41a3-a62c-f4c92fc3d3ed"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""68a6640a-7504-4f1b-b844-1a17f2baf661"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""a33fae7f-411b-49c7-a0ca-455b3af8ef2e"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""4a443bfa-3662-4f1d-8d0f-dc39409ddb03"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""a8dbac36-337e-4974-8fa2-250b622069b0"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveHover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""76041a07-fb91-4b7a-a442-ded1b0dac988"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DeselectAll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""074624f4-a09a-4d58-b85b-603a39b6258d"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DeselectAll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7fe3474d-0523-4c64-97df-c639483201f9"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PauseMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d722ba8d-b1a5-4d8c-beb0-2094b7c13bbd"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PauseMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // GameBoard
        m_GameBoard = asset.FindActionMap("GameBoard", throwIfNotFound: true);
        m_GameBoard_Select = m_GameBoard.FindAction("Select", throwIfNotFound: true);
        m_GameBoard_MoveHover = m_GameBoard.FindAction("MoveHover", throwIfNotFound: true);
        m_GameBoard_DeselectAll = m_GameBoard.FindAction("DeselectAll", throwIfNotFound: true);
        m_GameBoard_PauseMenu = m_GameBoard.FindAction("PauseMenu", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // GameBoard
    private readonly InputActionMap m_GameBoard;
    private IGameBoardActions m_GameBoardActionsCallbackInterface;
    private readonly InputAction m_GameBoard_Select;
    private readonly InputAction m_GameBoard_MoveHover;
    private readonly InputAction m_GameBoard_DeselectAll;
    private readonly InputAction m_GameBoard_PauseMenu;
    public struct GameBoardActions
    {
        private @PlayerControls m_Wrapper;
        public GameBoardActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Select => m_Wrapper.m_GameBoard_Select;
        public InputAction @MoveHover => m_Wrapper.m_GameBoard_MoveHover;
        public InputAction @DeselectAll => m_Wrapper.m_GameBoard_DeselectAll;
        public InputAction @PauseMenu => m_Wrapper.m_GameBoard_PauseMenu;
        public InputActionMap Get() { return m_Wrapper.m_GameBoard; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameBoardActions set) { return set.Get(); }
        public void SetCallbacks(IGameBoardActions instance)
        {
            if (m_Wrapper.m_GameBoardActionsCallbackInterface != null)
            {
                @Select.started -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnSelect;
                @Select.performed -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnSelect;
                @Select.canceled -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnSelect;
                @MoveHover.started -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnMoveHover;
                @MoveHover.performed -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnMoveHover;
                @MoveHover.canceled -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnMoveHover;
                @DeselectAll.started -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnDeselectAll;
                @DeselectAll.performed -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnDeselectAll;
                @DeselectAll.canceled -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnDeselectAll;
                @PauseMenu.started -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnPauseMenu;
                @PauseMenu.performed -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnPauseMenu;
                @PauseMenu.canceled -= m_Wrapper.m_GameBoardActionsCallbackInterface.OnPauseMenu;
            }
            m_Wrapper.m_GameBoardActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Select.started += instance.OnSelect;
                @Select.performed += instance.OnSelect;
                @Select.canceled += instance.OnSelect;
                @MoveHover.started += instance.OnMoveHover;
                @MoveHover.performed += instance.OnMoveHover;
                @MoveHover.canceled += instance.OnMoveHover;
                @DeselectAll.started += instance.OnDeselectAll;
                @DeselectAll.performed += instance.OnDeselectAll;
                @DeselectAll.canceled += instance.OnDeselectAll;
                @PauseMenu.started += instance.OnPauseMenu;
                @PauseMenu.performed += instance.OnPauseMenu;
                @PauseMenu.canceled += instance.OnPauseMenu;
            }
        }
    }
    public GameBoardActions @GameBoard => new GameBoardActions(this);
    public interface IGameBoardActions
    {
        void OnSelect(InputAction.CallbackContext context);
        void OnMoveHover(InputAction.CallbackContext context);
        void OnDeselectAll(InputAction.CallbackContext context);
        void OnPauseMenu(InputAction.CallbackContext context);
    }
}
