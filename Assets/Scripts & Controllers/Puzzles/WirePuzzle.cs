using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The electrical box mini-game: drag each loose wire on the left to the socket of the same colour on the right.
// It's a full-screen prefab (Assets/Prefabs/UI/Wire Puzzle) with its own canvas. ElectricalBox spawns it and
// calls Open(); it destroys itself when it closes. The colours are shuffled every time it opens.
public class WirePuzzle : MonoBehaviour
{
    [Header("Wires (one per row, top to bottom)")]
    [SerializeField] private WireDragEnd[] wires;
    [SerializeField] private WireSocket[] sockets;
    [SerializeField] private Color[] wireColors =
    {
        new Color(0.9f, 0.15f, 0.15f),
        new Color(0.2f, 0.35f, 0.95f),
        new Color(0.95f, 0.85f, 0.1f),
        new Color(0.9f, 0.2f, 0.85f)
    };

    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Text")]
    [SerializeField] private string instructions = "Drag each wire to the socket of the same colour";
    [SerializeField] private string solvedText = "Power restored!";

    [Header("Settings")]
    [SerializeField] private float closeDelayAfterSolved = 1f;

    private Action onSolved;
    private int connectedCount;
    private bool isSolved;

    public bool IsSolved => isSolved;

    private void Awake()
    {
        closeButton.onClick.AddListener(Cancel);
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }

    public void Open(Action onSolvedCallback)
    {
        onSolved = onSolvedCallback;
        connectedCount = 0;
        isSolved = false;
        statusLabel.text = instructions;
        PlayerLock.Add(this);

        int count = Mathf.Min(wires.Length, sockets.Length, wireColors.Length);
        int[] leftOrder = ShuffledOrder(count);
        int[] rightOrder = ShuffledOrder(count);
        for (int tries = 0; tries < 10 && count > 1 && SameOrder(leftOrder, rightOrder); tries++) rightOrder = ShuffledOrder(count);

        for (int i = 0; i < count; i++)
        {
            wires[i].Setup(this, leftOrder[i], wireColors[leftOrder[i]]);
            sockets[i].Setup(rightOrder[i], wireColors[rightOrder[i]]);
        }
    }

    public void OnWireConnected()
    {
        connectedCount++;
        if (!isSolved && connectedCount >= Mathf.Min(wires.Length, sockets.Length)) StartCoroutine(Solve());
    }

    // The X button: leave without fixing anything. The box can be opened again.
    public void Cancel()
    {
        if (!isSolved) Close();
    }

    private IEnumerator Solve()
    {
        isSolved = true;
        statusLabel.text = solvedText;
        closeButton.interactable = false;
        yield return new WaitForSecondsRealtime(closeDelayAfterSolved);

        Action solved = onSolved;
        Close();
        solved?.Invoke();
    }

    private void Close()
    {
        PlayerLock.Remove(this);
        Destroy(gameObject);
    }

    private static int[] ShuffledOrder(int count)
    {
        int[] order = new int[count];
        for (int i = 0; i < count; i++) order[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        return order;
    }

    private static bool SameOrder(int[] a, int[] b)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }
}
