using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [SerializeField] private List<RecipeSO> allRecipes;
    public RecipeSO SelectedRecipe { get; private set; }

    // 플레이어에게 공개될 힌트 재료 리스트
    public List<IngredientSO> revealedHints = new List<IngredientSO>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void SelectRandomRecipe()
    {
        if (allRecipes.Count == 0) return;

        // 1. 랜덤 레시피 선정
        SelectedRecipe = allRecipes[Random.Range(0, allRecipes.Count)];

        // 2. 힌트 생성 (2개만 무작위로 추출)
        GenerateHints();

        Debug.Log($"오늘의 요리: {SelectedRecipe.recipeName} (힌트 2개 추출 완료)");
    }

    private void GenerateHints()
    {
        revealedHints.Clear();

        // 레시피의 전체 재료 리스트를 복사해서 섞기
        List<IngredientAmount> tempIngredients = new List<IngredientAmount>(SelectedRecipe.requiredIngredients);

        // 간단한 셔플 로직
        for (int i = 0; i < tempIngredients.Count; i++)
        {
            int rnd = Random.Range(0, tempIngredients.Count);
            var temp = tempIngredients[i];
            tempIngredients[i] = tempIngredients[rnd];
            tempIngredients[rnd] = temp;
        }

        // 상위 revealCount개만 힌트로 선정
        for (int i = 0; i < SelectedRecipe.revealCount && i < tempIngredients.Count; i++)
        {
            revealedHints.Add(tempIngredients[i].ingredient);
        }
    }
}