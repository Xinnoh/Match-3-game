using System.Collections.Generic;
using UnityEngine;

public class MatchDetector : MonoBehaviour
{
    Gem gem;
    GridSystem grid;

    void Awake()
    {
        gem = GetComponent<Gem>();
        grid = FindObjectOfType<GridSystem>();
    }

    public List<Gem> GetMatches()
    {
        List<Gem> matched = new List<Gem>();
        if (gem == null || grid == null || !gem.canMatch) return matched;

        int gx = gem.x;
        int gy = gem.y;
        Gem center = grid.GetGemAt(gx, gy);
        if (center == null) return matched;

        List<Gem> matchedThisCheck = new List<Gem>();

        // Horizontal
        // Checks a direction. If it's match, add. If not, break.

        List<Gem> horizontal = new List<Gem> { center };
        for (int x = gx - 1; x >= 0; x--)
        {
            Gem g = grid.GetGemAt(x, gy);
            if (g != null && g.data == center.data && g.canMatch)
                horizontal.Add(g);
            else break;
        }
        for (int x = gx + 1; x < grid.Width; x++)
        {
            Gem g = grid.GetGemAt(x, gy);
            if (g != null && g.data == center.data && g.canMatch)
                horizontal.Add(g);
            else break;
        }
        if (horizontal.Count >= 3)
            matchedThisCheck.AddRange(horizontal);

        // Vertical
        List<Gem> vertical = new List<Gem> { center };
        for (int y = gy - 1; y >= 0; y--)
        {
            Gem g = grid.GetGemAt(gx, y);
            if (g != null && g.data == center.data && g.canMatch)
                vertical.Add(g);
            else break;
        }
        for (int y = gy + 1; y < grid.Height; y++)
        {
            Gem g = grid.GetGemAt(gx, y);
            if (g != null && g.data == center.data && g.canMatch)
                vertical.Add(g);
            else break;
        }
        if (vertical.Count >= 3)
            matchedThisCheck.AddRange(vertical);

        // Remove duplicates
        foreach (Gem g in matchedThisCheck)
        {
            if (!matched.Contains(g))
                matched.Add(g);
        }


        if (matched.Count >= 3)
        {
            // Trigger match effect on all gems in this match
            List<GemMatchEffect> effects = new List<GemMatchEffect>();
            foreach (Gem g in matched)
            {
                g.isMatched = true;
                if (g != center)
                    g.canMatch = false;

                var effect = g.GetComponent<GemMatchEffect>();
                if (effect != null)
                    effects.Add(effect);
            }

            // Add entire match to the queue
            MatchQueueManager.Instance.EnqueueMatch(effects);

            center.canMatch = false; // mark center as done
        }

        return matched;
    }



}
