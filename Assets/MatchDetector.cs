using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class MatchDetector : MonoBehaviour
{
    Gem gem;
    GridSystem grid;

    void Awake()
    {
        gem = GetComponent<Gem>();
        grid = FindFirstObjectByType<GridSystem>();
    }

    /// <summary>
    /// Finds primary match centered on this gem and enqueues it. 
    /// Then recursively check for orthogonal secondary matches 
    /// </summary>
    
    public List<Gem> GetMatches()
    {
        if (gem == null || grid == null || !gem.canMatch || gem.dragging)
            return new List<Gem>();

        int gx = gem.x;
        int gy = gem.y;
        Gem thisGem = grid.GetGemAt(gx, gy);

        if (thisGem == null || thisGem.dragging || thisGem.gemSO == null)
            return new List<Gem>();

        // --- 1. Find Primary Match ---
        List<Gem> horizontal = FindLineMatch(thisGem, gx, gy, 1, 0);
        List<Gem> vertical = FindLineMatch(thisGem, gx, gy, 0, 1);

        List<Gem> primaryMatched = new List<Gem>();
        bool isSpecialMatch = horizontal.Count >= 3 && vertical.Count >= 3;

        if (horizontal.Count >= 3)
            primaryMatched.AddRange(horizontal);
        if (vertical.Count >= 3)
            primaryMatched = primaryMatched.Union(vertical).ToList();

        // --- 2. Process and Enqueue Primary Match ---
        if (primaryMatched.Count >= 3)
        {
            if (isSpecialMatch)
            {
                List<Gem> firstMatch = (horizontal.Count >= vertical.Count) ? horizontal : vertical;
                List<Gem> secondMatch = (horizontal.Count >= vertical.Count) ? vertical : horizontal;

                // Convert gems to effects before queuing
                List<GemMatchEffect> firstEffects = firstMatch
                    .Select(g => g.GetComponent<GemMatchEffect>())
                    .Where(e => e != null)
                    .ToList();
                MatchQueueManager.Instance.EnqueueMatch(firstEffects, true);

                List<GemMatchEffect> secondEffects = secondMatch
                    .Select(g => g.GetComponent<GemMatchEffect>())
                    .Where(e => e != null)
                    .ToList();
                MatchQueueManager.Instance.EnqueueMatch(secondEffects, false);
            }
            else
            {
                List<GemMatchEffect> normalEffects = primaryMatched
                    .Select(g => g.GetComponent<GemMatchEffect>())
                    .Where(e => e != null)
                    .ToList();
                MatchQueueManager.Instance.EnqueueMatch(normalEffects, false);
            }

            // Mark all gems involved for future checks
            foreach (Gem g in primaryMatched)
            {
                g.isMatched = true;
                g.canMatch = false;
            }

            // --- Recursive Orthogonal Check ---
            foreach (Gem g in primaryMatched)
            {
                FindOrthogonalMatches(g);
            }
        }

        return primaryMatched;
    }


    /// <summary>
    /// Checks for a match orthogonal (opposite direction) to the one that 
    /// originally triggered this gem's inclusion, then enqueues it if found.
    /// </summary>
    /// <param name="g">The gem to check for an orthogonal match.</param>
    private void FindOrthogonalMatches(Gem g)
    {
        // Safety check: only check gems that were matched but are not fully processed yet
        if (g == null || g.gemSO == null || !g.isMatched) return;

        int gx = g.x;
        int gy = g.y;

        // Since we don't know if the gem was matched horizontally or vertically, 
        // we check both directions again.

        // Check Horizontal match 
        List<Gem> horizontal = FindLineMatch(g, gx, gy, 1, 0);

        // Check Vertical match
        List<Gem> vertical = FindLineMatch(g, gx, gy, 0, 1);

        List<Gem> orthogonalMatched = new List<Gem>();

        // IMPORTANT: We only care about secondary matches that are 3+ long AND 
        // that are *different* from the primary match direction.
        // Since primaryMatched already covered the T/L case centered on thisGem, 
        // this check is for secondary T/L/Cross matches centered on other gems.

        // Check horizontal (if it's a new 3+ match)
        if (horizontal.Count >= 3)
        {
            orthogonalMatched.AddRange(horizontal.Where(gem => !gem.isMatched));
        }

        // Check vertical (if it's a new 3+ match)
        if (vertical.Count >= 3)
        {
            orthogonalMatched = orthogonalMatched.Union(vertical.Where(gem => !gem.isMatched)).ToList();
        }

        // If a new, secondary match is found:
        if (orthogonalMatched.Count > 0)
        {
            List<GemMatchEffect> secondaryEffects = new List<GemMatchEffect>();

            foreach (Gem matchedGem in orthogonalMatched)
            {
                // Note: The gem that triggered this check (g) might be in the orthogonal list, 
                // but its effects should already be in the primary queue. 
                // We only need to process *new* gems that haven't been marked.
                if (!matchedGem.isMatched)
                {
                    matchedGem.isMatched = true;
                    matchedGem.canMatch = false;

                    var effect = matchedGem.GetComponent<GemMatchEffect>();
                    if (effect != null)
                        secondaryEffects.Add(effect);
                }
            }

            if (secondaryEffects.Count > 0)
            {
                // Enqueue the secondary match effects
                MatchQueueManager.Instance.EnqueueMatch(secondaryEffects, false);
            }

            // RECURSION: Check all newly matched gems for further orthogonal matches
            foreach (Gem nextGem in orthogonalMatched)
            {
                // We recursively check the newly matched gems for *their* orthogonal matches.
                // This handles a complex cascading match (e.g., a cross shape creating another cross shape).
                FindOrthogonalMatches(nextGem);
            }
        }
    }


    private List<Gem> FindLineMatch(Gem centerGem, int gx, int gy, int dx, int dy)
    {
        List<Gem> line = new List<Gem> { centerGem };

        // Check in the negative direction (e.g., left or down)
        for (int i = 1; ; i++)
        {
            int x = gx - i * dx;
            int y = gy - i * dy;

            // Grid boundary check
            if (!grid.InBounds(x, y))
                break;

            Gem g = grid.GetGemAt(x, y);
            if (g != null && g.gemSO == centerGem.gemSO && g.canMatch)
            {
                line.Add(g);
            }
            else
            {
                break;
            }
        }

        // Check in the positive direction (e.g., right or up)
        for (int i = 1; ; i++)
        {
            int x = gx + i * dx;
            int y = gy + i * dy;

            // Grid boundary check
            if (!grid.InBounds(x, y))
                break;

            Gem g = grid.GetGemAt(x, y);
            if (g != null && g.gemSO == centerGem.gemSO && g.canMatch)
            {
                line.Add(g);
            }
            else
            {
                break;
            }
        }

        return line;
    }
}