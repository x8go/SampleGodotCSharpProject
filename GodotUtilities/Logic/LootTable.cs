using System.Collections.Generic;
using System.Linq;

namespace GodotUtilities.Logic;

/// <summary>
/// 가중치 기반 랜덤 선택 테이블 (루트 테이블)
/// 각 아이템에 가중치를 부여하여 확률적으로 선택할 수 있습니다
/// </summary>
/// <typeparam name="T">테이블에 저장할 아이템 타입</typeparam>
public class LootTable<T>
{
	/// <summary>
	/// 모든 아이템 가중치의 합계
	/// </summary>
	public int WeightSum { get; protected set; }

	/// <summary>
	/// 아이템과 가중치를 저장하는 테이블
	/// </summary>
	private readonly List<TableData> table = new();

	/// <summary>
	/// 난수 생성기
	/// </summary>
	private RandomNumberGenerator random;

	/// <summary>
	/// 테이블 데이터 클래스 (아이템과 가중치를 함께 저장)
	/// </summary>
	public class TableData
	{
		/// <summary>
		/// 저장된 객체
		/// </summary>
		public T Obj { get; private set; }

		/// <summary>
		/// 아이템의 가중치 (높을수록 선택 확률이 높음)
		/// </summary>
		public int Weight { get; private set; }

		/// <summary>
		/// TableData 생성자
		/// </summary>
		/// <param name="o">저장할 객체</param>
		/// <param name="w">가중치</param>
		public TableData(T o, int w)
		{
			Obj = o;
			Weight = w;
		}
	}

	/// <summary>
	/// LootTable 생성자
	/// 전역 난수 생성기를 사용합니다
	/// </summary>
	public LootTable()
	{
		random = MathUtil.RNG;
	}

	/// <summary>
	/// 난수 생성기의 시드를 설정합니다
	/// </summary>
	/// <param name="seed">시드 값</param>
	public void SetSeed(ulong seed)
	{
		random.Seed = seed;
	}

	/// <summary>
	/// 사용할 난수 생성기를 설정합니다
	/// </summary>
	/// <param name="random">난수 생성기</param>
	public void SetRandom(RandomNumberGenerator random)
	{
		this.random = random;
	}

	/// <summary>
	/// 아이템과 가중치를 테이블에 추가합니다
	/// </summary>
	/// <param name="obj">추가할 아이템</param>
	/// <param name="weight">아이템의 가중치</param>
	public void AddItem(T obj, int weight)
	{
		table.Add(new TableData(obj, weight));
		CalculateWeightSum(); // 가중치 합계 재계산
	}

	/// <summary>
	/// TableData를 테이블에 추가합니다
	/// </summary>
	/// <param name="tableData">추가할 테이블 데이터</param>
	public void AddItem(TableData tableData)
	{
		table.Add(tableData);
		CalculateWeightSum(); // 가중치 합계 재계산
	}

	/// <summary>
	/// 여러 TableData를 한 번에 추가합니다
	/// </summary>
	/// <param name="range">추가할 테이블 데이터 리스트</param>
	public void AddRange(List<TableData> range)
	{
		table.AddRange(range);
		CalculateWeightSum(); // 가중치 합계 재계산
	}

	/// <summary>
	/// 테이블의 모든 데이터를 새로운 데이터로 교체합니다
	/// </summary>
	/// <param name="tableData">새로운 테이블 데이터 리스트</param>
	public void SetData(List<TableData> tableData)
	{
		table.Clear();
		AddRange(tableData);
	}

	/// <summary>
	/// 가중치에 따라 랜덤하게 아이템을 선택합니다
	/// </summary>
	/// <returns>선택된 아이템</returns>
	public T PickItem()
	{
		return PickItem(table, WeightSum);
	}

	/// <summary>
	/// 가중치에 따라 랜덤하게 TableData를 선택합니다
	/// </summary>
	/// <returns>선택된 TableData (무한 루프 주의: 현재 구현은 자기 자신을 호출)</returns>
	public TableData PickTableData()
	{
		// 주의: 원본 코드의 버그 - 무한 재귀 발생 가능성
		return PickTableData();
	}

	/// <summary>
	/// 조건을 만족하는 아이템을 선택합니다
	/// 조건을 만족할 때까지 계속 선택을 시도하며, 선택된 아이템은 임시로 제외됩니다
	/// </summary>
	/// <param name="canPickConditionalFn">아이템을 선택할 수 있는지 판단하는 함수</param>
	/// <returns>조건을 만족하는 아이템, 또는 없으면 기본값</returns>
	public T PickItemConditional(Func<TableData, bool> canPickConditionalFn)
	{
		// 테이블 복사본 생성 (원본 테이블은 수정하지 않음)
		var tableCopy = table.ToList();
		var weightSum = WeightSum;

		TableData pickedTableData;
		do
		{
			// 아이템 선택
			pickedTableData = PickTableData(tableCopy, weightSum);
			if (pickedTableData != null)
			{
				// 선택된 아이템의 가중치를 합계에서 제외
				weightSum -= pickedTableData.Weight;
				// 선택된 아이템을 임시 테이블에서 제거
				tableCopy.Remove(pickedTableData);
			}
		} while (pickedTableData != null && !canPickConditionalFn(pickedTableData)); // 조건을 만족할 때까지 반복

		return pickedTableData != null ? pickedTableData.Obj : default;
	}

	/// <summary>
	/// 테이블의 특정 범위 내에서 랜덤하게 아이템을 선택합니다
	/// </summary>
	/// <param name="startIdx">시작 인덱스</param>
	/// <param name="count">선택할 범위의 개수</param>
	/// <returns>선택된 아이템</returns>
	public T PickRange(int startIdx, int count)
	{
		// 지정된 범위의 데이터 추출
		var range = table.GetRange(startIdx, count);
		// 범위 내 가중치 합계 계산
		var weightSum = range.Sum(x => x.Weight);
		return PickItem(range, weightSum);
	}

	/// <summary>
	/// 테이블의 모든 아이템을 리스트로 반환합니다
	/// </summary>
	/// <returns>아이템 리스트</returns>
	public List<T> GetLootTableItems()
	{
		return table.Select(x => x.Obj).ToList();
	}

	/// <summary>
	/// 테이블의 모든 TableData를 리스트로 반환합니다
	/// </summary>
	/// <returns>TableData 리스트</returns>
	public List<TableData> GetLootTableData()
	{
		return table;
	}

	/// <summary>
	/// 테이블에 저장된 아이템의 개수를 반환합니다
	/// </summary>
	/// <returns>아이템 개수</returns>
	public int GetCount()
	{
		return table.Count;
	}

	/// <summary>
	/// 모든 아이템의 가중치 합계를 계산합니다
	/// </summary>
	public void CalculateWeightSum()
	{
		WeightSum = table.Sum(x => x.Weight);
	}

	/// <summary>
	/// 지정된 테이블에서 가중치에 따라 아이템을 선택합니다 (내부 메서드)
	/// </summary>
	/// <param name="table">선택할 테이블</param>
	/// <param name="weightSum">가중치 합계</param>
	/// <returns>선택된 아이템</returns>
	private T PickItem(List<TableData> table, int weightSum)
	{
		var tableData = PickTableData(table, weightSum);
		return tableData != null ? tableData.Obj : default;
	}

	/// <summary>
	/// 지정된 테이블에서 가중치에 따라 TableData를 선택합니다 (내부 메서드)
	/// 가중치 기반 확률 선택 알고리즘을 구현합니다
	/// </summary>
	/// <param name="table">선택할 테이블</param>
	/// <param name="weightSum">가중치 합계</param>
	/// <returns>선택된 TableData</returns>
	private TableData PickTableData(List<TableData> table, int weightSum)
	{
		int sum = 0;
		// 1부터 가중치 합계까지의 랜덤 값 생성
		int val = random.RandiRange(1, weightSum);
		foreach (var data in table)
		{
			// 누적 가중치 계산
			sum += data.Weight;
			// 랜덤 값이 누적 가중치 이하이면 해당 아이템 선택
			if (val <= sum)
			{
				return data;
			}
		}
		return null;
	}
}
