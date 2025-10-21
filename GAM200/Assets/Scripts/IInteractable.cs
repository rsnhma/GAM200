using UnityEngine;

// Interface for all interactable objects in the game
public interface IInteractable
{
    // Called when the player interacts with this object
 
    void Interact();

    // Returns the text to display as interaction prompt
    string GetInteractionPrompt();

    // Returns the maximum distance from which player can interact

    float GetInteractionRange();
}