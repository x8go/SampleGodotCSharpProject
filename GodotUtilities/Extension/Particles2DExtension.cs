namespace GodotUtilities;

/// <summary>
/// GpuParticles2D 클래스의 확장 메서드를 제공하는 정적 클래스
/// 2D 파티클 시스템의 속성을 보다 편리하게 제어할 수 있습니다
/// </summary>
public static class Particles2DExtension
{
	/// <summary>
	/// GpuParticles2D의 확장 메서드: 파티클의 방향을 설정합니다
	/// </summary>
	/// <param name="particles">파티클 시스템</param>
	/// <param name="direction">설정할 방향 (Vector2)</param>
	public static void SetDirection(this GpuParticles2D particles, Vector2 direction)
	{
		// ProcessMaterial이 ParticleProcessMaterial인 경우에만 방향 설정
		if (particles.ProcessMaterial is ParticleProcessMaterial material)
		{
			// 2D 벡터를 3D 벡터로 변환하여 방향 설정
			material.Direction = new Vector3(direction.X, direction.Y, 0f);
		}
	}
}
