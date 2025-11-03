using System.Collections.Generic;

namespace GodotUtilities.Util;

/// <summary>
/// 파일 시스템 관련 유틸리티 메서드를 제공하는 정적 클래스
/// 씬 로딩 및 리소스 로딩을 편리하게 할 수 있습니다
/// </summary>
public static class FileSystem
{
	/// <summary>
	/// 지정된 디렉토리의 모든 씬 파일(.tscn)을 인스턴스화하여 리스트로 반환합니다
	/// </summary>
	/// <typeparam name="T">인스턴스화할 노드 타입</typeparam>
	/// <param name="dirPath">씬 파일들이 있는 디렉토리 경로</param>
	/// <returns>인스턴스화된 씬들의 리스트</returns>
	public static List<T> InstanceScenesInPath<T>(string dirPath) where T : Node
	{
		// 경로 끝에 슬래시 추가
		if (dirPath[^1] != '/')
		{
			dirPath += "/";
		}

		var scenes = new List<T>();

		// 디렉토리 열기
		using var dir = DirAccess.Open(dirPath);
		if (dir == null)
		{
			Logger.Error("Could not open directory " + dirPath);
			return scenes;
		}

		// 디렉토리 파일 목록 순회 시작
		dir.ListDirBegin();
		while (true)
		{
			var path = dir.GetNext();
			// 더 이상 파일이 없으면 종료
			if (string.IsNullOrEmpty(path))
			{
				break;
			}
			// .tscn 파일만 처리 (.converted.res 제외)
			if (!path.Contains(".converted.res") && path.Contains(".tscn"))
			{
				// .remap 확장자 제거
				path = path.Replace(".remap", "");
				var fullPath = dirPath + path;

				// PackedScene 로드 및 인스턴스화
				if (GD.Load(fullPath) is PackedScene packedScene)
				{
					var scene = packedScene.Instantiate();
					// 타입이 일치하면 리스트에 추가
					if (scene is T node)
					{
						scenes.Add(node);
					}
					else
					{
						// 타입이 일치하지 않으면 메모리 해제
						scene.QueueFree();
					}
				}
			}
		}
		// 디렉토리 순회 종료
		dir.ListDirEnd();

		return scenes;
	}

	/// <summary>
	/// 지정된 디렉토리의 모든 리소스 파일(.tres, .converted.res)을 로드하여 리스트로 반환합니다
	/// </summary>
	/// <typeparam name="T">로드할 리소스 타입</typeparam>
	/// <param name="path">리소스 파일들이 있는 디렉토리 경로</param>
	/// <returns>로드된 리소스들의 리스트</returns>
	public static List<T> LoadResourcesInPath<T>(string path) where T : Resource
	{
		// 디렉토리 열기
		using var dir = DirAccess.Open(path);
		var results = new List<T>();
		if (dir != null)
		{
			// 디렉토리 파일 목록 순회 시작
			dir.ListDirBegin();
			var fileName = dir.GetNext();
			while (fileName != string.Empty)
			{
				// 디렉토리가 아닌 파일만 처리
				if (!dir.CurrentIsDir())
				{
					// .tres 또는 .converted.res 파일만 처리
					if (fileName.EndsWith(".converted.res") || fileName.EndsWith(".tres"))
					{
						// .converted.res 확장자 제거
						fileName = fileName.Replace(".converted.res", string.Empty);
						var fullPath = $"{path}/{fileName}";
						// 리소스 로드
						var resource = GD.Load(fullPath);
						// 타입이 일치하지 않으면 경고 출력 및 스킵
						if (resource is not T res)
						{
							GD.PushWarning($"Could not load resource at {fullPath} with type {typeof(T).Name}");
							continue;
						}
						// 타입이 일치하면 리스트에 추가
						results.Add(res);
					}
				}
				// 다음 파일로 이동
				fileName = dir.GetNext();
			}
			// 디렉토리 순회 종료
			dir.ListDirEnd();
		}
		else
		{
			// 디렉토리를 열 수 없으면 경고 출력
			GD.PushWarning($"Could load resources in path {path}");
		}
		return results;
	}
}
