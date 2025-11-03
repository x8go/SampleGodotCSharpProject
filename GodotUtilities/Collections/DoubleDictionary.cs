using System.Collections;
using System.Collections.Generic;

namespace GodotUtilities.Collections;

/// <summary>
/// 양방향 딕셔너리 구현체
/// Key로 Value를 찾거나 Value로 Key를 찾을 수 있는 이중 매핑 자료구조
/// </summary>
/// <typeparam name="TKey">키의 타입</typeparam>
/// <typeparam name="TValue">값의 타입</typeparam>
public class DoubleDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
	/// <summary>
	/// 키에서 값으로의 매핑을 저장하는 딕셔너리
	/// </summary>
	private readonly Dictionary<TKey, TValue> _keyToValue = new();

	/// <summary>
	/// 값에서 키로의 매핑을 저장하는 딕셔너리 (역방향 검색용)
	/// </summary>
	private readonly Dictionary<TValue, TKey> _valueToKey = new();

	/// <summary>
	/// 키를 사용하여 값에 접근하는 인덱서
	/// </summary>
	/// <param name="key">검색할 키</param>
	/// <returns>키에 대응하는 값</returns>
	public TValue this[TKey key]
	{
		get => _keyToValue[key];
		set
		{
			// 기존 키가 있으면 이전 값의 역방향 매핑 제거
			if (_keyToValue.ContainsKey(key))
			{
				var oldVal = _keyToValue[key];
				_valueToKey.Remove(oldVal);
			}
			// 양방향 매핑 추가
			_keyToValue[key] = value;
			_valueToKey[value] = key;
		}
	}

	/// <summary>
	/// 값을 사용하여 키에 접근하는 인덱서 (역방향 검색)
	/// </summary>
	/// <param name="val">검색할 값</param>
	/// <returns>값에 대응하는 키</returns>
	public TKey this[TValue val]
	{
		get => _valueToKey[val];
		set
		{
			// 기존 값이 있으면 이전 키의 정방향 매핑 제거
			if (_valueToKey.ContainsKey(val))
			{
				var oldVal = _valueToKey[val];
				_keyToValue.Remove(oldVal);
			}
			// 양방향 매핑 추가
			_valueToKey[val] = value;
			_keyToValue[value] = val;
		}
	}

	/// <summary>
	/// 모든 키의 컬렉션
	/// </summary>
	public ICollection<TKey> Keys => _keyToValue.Keys;

	/// <summary>
	/// 모든 값의 컬렉션 (_valueToKey의 키가 실제 값이므로 해당 Keys 사용)
	/// </summary>
	public ICollection<TValue> Values => _valueToKey.Keys;

	/// <summary>
	/// 딕셔너리에 저장된 항목의 개수
	/// </summary>
	public int Count => _keyToValue.Count;

	/// <summary>
	/// 읽기 전용 여부 (항상 false)
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// 키-값 쌍을 딕셔너리에 추가합니다 (양방향 매핑)
	/// </summary>
	/// <param name="key">추가할 키</param>
	/// <param name="value">추가할 값</param>
	public void Add(TKey key, TValue value)
	{
		// 양방향 매핑 추가
		_keyToValue[key] = value;
		_valueToKey[value] = key;
	}

	/// <summary>
	/// KeyValuePair를 딕셔너리에 추가합니다
	/// </summary>
	/// <param name="item">추가할 키-값 쌍</param>
	public void Add(KeyValuePair<TKey, TValue> item)
	{
		_keyToValue[item.Key] = item.Value;
		_valueToKey[item.Value] = item.Key;
	}

	/// <summary>
	/// 딕셔너리의 모든 항목을 제거합니다
	/// </summary>
	public void Clear()
	{
		_keyToValue.Clear();
		_valueToKey.Clear();
	}

	/// <summary>
	/// 지정된 키-값 쌍이 양방향으로 모두 존재하는지 확인합니다
	/// </summary>
	/// <param name="item">확인할 키-값 쌍</param>
	/// <returns>존재하면 true, 아니면 false</returns>
	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return _keyToValue.ContainsKey(item.Key) && _valueToKey.ContainsKey(item.Value);
	}

	/// <summary>
	/// 지정된 키가 딕셔너리에 존재하는지 확인합니다
	/// </summary>
	/// <param name="key">확인할 키</param>
	/// <returns>존재하면 true, 아니면 false</returns>
	public bool ContainsKey(TKey key)
	{
		return _keyToValue.ContainsKey(key);
	}

	/// <summary>
	/// 지정된 값이 딕셔너리에 존재하는지 확인합니다 (역방향 검색)
	/// </summary>
	/// <param name="value">확인할 값</param>
	/// <returns>존재하면 true, 아니면 false</returns>
	public bool ContainsKey(TValue value)
	{
		return _valueToKey.ContainsKey(value);
	}

	/// <summary>
	/// 딕셔너리의 항목들을 배열에 복사합니다 (구현 없음)
	/// </summary>
	/// <param name="array">대상 배열</param>
	/// <param name="arrayIndex">시작 인덱스</param>
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{ }

	/// <summary>
	/// 딕셔너리를 반복하기 위한 열거자를 반환합니다
	/// </summary>
	/// <returns>열거자</returns>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return _keyToValue.GetEnumerator();
	}

	/// <summary>
	/// 지정된 키와 해당하는 값을 양방향으로 제거합니다
	/// </summary>
	/// <param name="key">제거할 키</param>
	/// <returns>제거 성공 시 true, 키가 없으면 false</returns>
	public bool Remove(TKey key)
	{
		if (_keyToValue.ContainsKey(key))
		{
			// 양방향 매핑 모두 제거
			var val = _keyToValue[key];
			_keyToValue.Remove(key);
			_valueToKey.Remove(val);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 지정된 값과 해당하는 키를 양방향으로 제거합니다 (역방향 제거)
	/// </summary>
	/// <param name="value">제거할 값</param>
	/// <returns>제거 성공 시 true, 값이 없으면 false</returns>
	public bool Remove(TValue value)
	{
		if (_valueToKey.ContainsKey(value))
		{
			// 양방향 매핑 모두 제거
			var key = _valueToKey[value];
			_valueToKey.Remove(value);
			_keyToValue.Remove(key);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 지정된 키-값 쌍을 제거합니다
	/// </summary>
	/// <param name="item">제거할 키-값 쌍</param>
	/// <returns>제거 성공 시 true, 실패 시 false</returns>
	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return Remove(item.Key);
	}

	/// <summary>
	/// 키로 값을 검색합니다
	/// </summary>
	/// <param name="key">검색할 키</param>
	/// <param name="value">출력될 값</param>
	/// <returns>키가 존재하면 true, 아니면 false</returns>
	public bool TryGetValue(TKey key, out TValue value)
	{
		if (_keyToValue.ContainsKey(key))
		{
			value = _keyToValue[key];
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// 값으로 키를 검색합니다 (역방향 검색)
	/// </summary>
	/// <param name="value">검색할 값</param>
	/// <param name="key">출력될 키</param>
	/// <returns>값이 존재하면 true, 아니면 false</returns>
	public bool TryGetValue(TValue value, out TKey key)
	{
		if (_valueToKey.ContainsKey(value))
		{
			key = _valueToKey[value];
			return true;
		}
		key = default;
		return false;
	}

	/// <summary>
	/// 비제네릭 열거자를 반환합니다
	/// </summary>
	/// <returns>열거자</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _keyToValue.GetEnumerator();
	}
}
