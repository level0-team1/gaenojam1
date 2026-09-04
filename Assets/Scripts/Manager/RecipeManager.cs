using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [SerializeField] private List<RecipeSO> allRecipes;
    public RecipeSO SelectedRecipe { get; private set; }
    public IReadOnlyList<RecipeSO> AllRecipes => allRecipes;

    // �÷��̾�� ������ ��Ʈ ��� ����Ʈ
    public List<IngredientSO> revealedHints = new List<IngredientSO>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void SelectRandomRecipe()
    {
        if (allRecipes.Count == 0) return;

        // 1. ���� ������ ����
        SelectedRecipe = allRecipes[Random.Range(0, allRecipes.Count)];

        // 2. ��Ʈ ���� (2���� �������� ����)
        GenerateHints();

        Debug.Log($"������ �丮: {SelectedRecipe.recipeName} (��Ʈ 2�� ���� �Ϸ�)");
    }

    private void GenerateHints()
    {
        revealedHints.Clear();

        // �������� ��ü ��� ����Ʈ�� �����ؼ� ����
        List<IngredientAmount> tempIngredients = new List<IngredientAmount>(SelectedRecipe.requiredIngredients);

        // ������ ���� ����
        for (int i = 0; i < tempIngredients.Count; i++)
        {
            int rnd = Random.Range(0, tempIngredients.Count);
            var temp = tempIngredients[i];
            tempIngredients[i] = tempIngredients[rnd];
            tempIngredients[rnd] = temp;
        }

        // ���� revealCount���� ��Ʈ�� ����
        for (int i = 0; i < SelectedRecipe.revealCount && i < tempIngredients.Count; i++)
        {
            revealedHints.Add(tempIngredients[i].ingredient);
        }
    }
}