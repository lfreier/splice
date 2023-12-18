using System;
using System.Collections;
using UnityEngine;

public class Controller2D : MonoBehaviour
{
	const float skinWidth = 0.015F;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	public Collider2D collider2DObj;
	public LayerMask collisionMask;

	CollisionData collisionData;
	RaycastOrigins raycastOrigins;

	// Use this for initialization
	void Start()
	{
		CalculateRaySpacing();
	}

	public void Move(Vector3 velocity)
	{
		CheckCollisions(velocity);
		transform.Translate(velocity, Space.World);
	}

	public void MoveRect(Vector3 velocity)
	{
		CheckCollisionsRect(velocity);
		transform.Translate(velocity, Space.World);
	}

	/* Move relative to the object itself, not the world. */
	public void Move2(Vector3 velocity)
	{
		CheckCollisions(velocity);
		transform.Translate(velocity);
	}

	private void CheckCollisions(Vector3 velocity)
	{
		UpdateRaycastOrigins();
		collisionData.Reset();

		if (velocity.x != 0)
		{
			HorizontalCollisions(ref velocity);
		}

		if (velocity.y != 0)
		{
			VerticalCollisions(ref velocity);
		}
	}

	private void CheckCollisionsRect(Vector3 velocity)
	{
		UpdateRaycastOriginsRect();
		collisionData.Reset();

		if (velocity.x != 0)
		{
			HorizontalCollisions(ref velocity);
		}

		if (velocity.y != 0)
		{
			VerticalCollisions(ref velocity);
		}
	}

	void HorizontalCollisions(ref Vector3 movement)
	{
		int i = 0;
		float directionX = Mathf.Sign(movement.x);
		float rayLength = Mathf.Abs(movement.x) + skinWidth;

		for (i = 0; i < horizontalRayCount; i++)
		{
			Vector2 rayOrigin = (directionX < 0) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;

			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX);

			if (hit)
			{
				movement.x = (hit.distance - skinWidth) * directionX;
				rayLength = hit.distance;
				collisionData.left = directionX < 0;
				collisionData.right = directionX > 0;
			}
		}
	}

	void VerticalCollisions(ref Vector3 movement)
	{
		int i = 0;
		float directionY = Mathf.Sign(movement.y);
		float rayLength = Mathf.Abs(movement.y) + skinWidth;

		for (i = 0; i < verticalRayCount; i++)
		{
			Vector2 rayOrigin = (directionY < 0) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;

			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY);

			if (hit)
			{
				movement.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;
				collisionData.below = directionY < 0;
				collisionData.above = directionY > 0;
			}
		}
	}

	void UpdateRaycastOrigins()
	{
		Bounds bounds = collider2DObj.bounds;
		bounds.Expand(skinWidth * -4);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
	}

	void UpdateRaycastOriginsRect()
	{
		Bounds bounds = collider2DObj.bounds;
		float maxX, minX, maxY, minY;
		bounds.Expand(skinWidth * -4);
		maxX = bounds.max.y > bounds.max.x ? bounds.max.y : bounds.max.x;
		minX = bounds.min.y < bounds.min.x ? bounds.min.y : bounds.min.x;
		maxY = bounds.max.x > bounds.max.y ? bounds.max.x : bounds.max.y;
		minY = bounds.min.x < bounds.min.y ? bounds.min.x : bounds.min.y;

		raycastOrigins.topRight = new Vector2(maxX, maxY);
		raycastOrigins.topLeft = new Vector2(minX, maxY);
		raycastOrigins.bottomRight = new Vector2(maxX, minY);
		raycastOrigins.bottomLeft = new Vector2(minX, minY);
	}

	void CalculateRaySpacing()
	{
		Bounds bounds = collider2DObj.bounds;
		bounds.Expand(skinWidth * -2);

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	struct RaycastOrigins
	{
		public Vector2 topRight;
		public Vector2 topLeft;
		public Vector2 bottomRight;
		public Vector2 bottomLeft;
	}

	public struct CollisionData
	{
		public bool above;
		public bool below;
		public bool left;
		public bool right;

		public void Reset()
		{
			above = below = false;
			left = right = false;
		}
	}
}