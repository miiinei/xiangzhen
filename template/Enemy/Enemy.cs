using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Enemy : Piece
{
	private const float DefaultPlayerTargetScore = 5f;
	private const float DefaultFriendlyFireScore = -2f;
	private const float DefaultRepeatPenalty = -1.5f;
	private const float DefaultApproachWeight = 0.1f;
	private const float DefaultSuboptimalChance = 0.05f;
	private const string ScoreRangeHint = "-20.0,20.0,0.1";
	private const string ProbabilityRangeHint = "0.0,1.0,0.01";
	private const string WeightRangeHint = "0.0,5.0,0.05";
	private const string SpeedRangeHint = "0,100,1";
	private const float ScoreComparisonTolerance = 0.001f;
	private const int DefaultSpeed = 5;
	private const int ActionOrderFontSize = 28;
	private const float ActionOrderLabelSize = 40f;
	private const float ActionOrderWorldElevation = 0.9f;
	private static readonly Vector2 ActionOrderScreenOffset = new(0f, -20f);
	private static readonly Color ActionOrderFontColor = new(1f, 0.85f, 0.25f, 1f);

	[Export(PropertyHint.Range, ScoreRangeHint)] public float PlayerTargetScore { get; set; } = DefaultPlayerTargetScore;
	[Export(PropertyHint.Range, ScoreRangeHint)] public float FriendlyFireScore { get; set; } = DefaultFriendlyFireScore;
	[Export(PropertyHint.Range, ScoreRangeHint)] public float RepeatActionPenalty { get; set; } = DefaultRepeatPenalty;
	[Export(PropertyHint.Range, WeightRangeHint)] public float ApproachWeight { get; set; } = DefaultApproachWeight;
	[Export(PropertyHint.Range, ProbabilityRangeHint)] public float SuboptimalActionChance { get; set; } = DefaultSuboptimalChance;
	[Export(PropertyHint.Range, SpeedRangeHint)] public int Speed { get; set; } = DefaultSpeed;

	private Skill _preparedSkill;
	private Tile _preparedTarget;
	private Vector2I _preparedTargetOffset;
	private Skill _lastPreparedSkill;
	private Vector2I _lastPreparedTargetOffset;
	private bool _hasPreviousAction;
	private Label _actionOrderLabel;

	private sealed class ActionCandidate
	{
		public Tile MoveDestination { get; init; }
		public Skill Skill { get; init; }
		public Tile Target { get; init; }
		public float Score { get; init; }
	}

	public override void _Ready()
	{
		Faction = Faction.Enemy;
		base._Ready();

		foreach (var skill in GetSkills())
		{
			if (skill == null || skill.GetParent() != null) continue;
			AddChild(skill);
			skill.Visible = false;
		}

		CreateActionOrderLabel();
	}

	public override void Select()
	{
	}

	public void PrepareInitialAttack() => PrepareAction(ChooseAction(false));

	public void ExecutePreparedAttack()
	{
		if (IsDead || !RefreshPreparedTarget()) return;

		Skill skill = _preparedSkill;
		Tile target = _preparedTarget;
		_preparedSkill = null;
		_preparedTarget = null;
		skill.Execute(target);
	}

	public void RefreshPreparedAttackPreview()
	{
		if (IsDead || !RefreshPreparedTarget()) return;
		_preparedSkill.ShowCombatPreview(_preparedTarget);
	}

	public async Task MoveAndPrepareAsync()
	{
		if (IsDead) return;

		ResetTurn();
		ActionCandidate action = ChooseAction(true);
		if (action == null) return;

		if (action.MoveDestination != OccupiedTile)
		{
			var path = FindPath(action.MoveDestination);
			if (path != null)
			{
				RemainingMoveRange = Math.Max(0, RemainingMoveRange - path.Count + 1);
				await MoveAlongPathAsync(path);
			}
		}

		if (!IsDead) PrepareAction(action);
		RefreshActionOrderPosition();
	}

	public override void Die()
	{
		_preparedSkill?.ClearCombatPreview();
		_preparedSkill = null;
		_preparedTarget = null;
		_actionOrderLabel?.QueueFree();
		base.Die();
	}

	public void SetActionOrder(int actionOrder)
	{
		if (_actionOrderLabel == null) return;
		_actionOrderLabel.Text = actionOrder.ToString();
		RefreshActionOrderPosition();
	}

	public void SetActionOrderVisible(bool visible)
	{
		if (_actionOrderLabel != null) _actionOrderLabel.Visible = visible;
	}

	public void RefreshActionOrderPosition()
	{
		if (_actionOrderLabel == null || WorldRef == null) return;
		if (!WorldRef.TryProjectToScreen(GlobalPosition + Vector3.Up * ActionOrderWorldElevation, out Vector2 screenPosition))
		{
			_actionOrderLabel.Visible = false;
			return;
		}

		_actionOrderLabel.Position = screenPosition + ActionOrderScreenOffset - _actionOrderLabel.Size * HexGeometry.Half;
	}

	private void CreateActionOrderLabel()
	{
		if (WorldRef?.UiLayer == null) return;
		_actionOrderLabel = new Label
		{
			Name = SceneNames.EnemyActionOrder(Name),
			Visible = false,
			CustomMinimumSize = new Vector2(ActionOrderLabelSize, ActionOrderLabelSize),
			Size = new Vector2(ActionOrderLabelSize, ActionOrderLabelSize),
			MouseFilter = Control.MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		_actionOrderLabel.AddThemeColorOverride(GodotThemeKeys.FontColor, ActionOrderFontColor);
		_actionOrderLabel.AddThemeFontSizeOverride(GodotThemeKeys.FontSize, ActionOrderFontSize);
		WorldRef.UiLayer.AddChild(_actionOrderLabel);
	}

	private ActionCandidate ChooseAction(bool includeMovement)
	{
		if (WorldRef == null || OccupiedTile == null) return null;

		var moveDestinations = new List<Tile> { OccupiedTile };
		if (includeMovement)
		{
			foreach (var position in GetReachableTiles())
			{
				var tile = WorldRef.GetTile(position);
				if (tile != null) moveDestinations.Add(tile);
			}
		}

		var candidates = new List<ActionCandidate>();
		foreach (var moveDestination in moveDestinations)
		{
			foreach (var skill in GetSkills())
			{
				if (skill == null) continue;
				foreach (var target in WorldRef.Tiles.Values)
				{
					if (!skill.CanTargetFrom(moveDestination, target)) continue;
					var predictedDamage = skill.PredictDamage(moveDestination, target);
					if (!DealsDamageToPlayer(predictedDamage)) continue;
					candidates.Add(new ActionCandidate
					{
						MoveDestination = moveDestination,
						Skill = skill,
						Target = target,
						Score = ScoreAction(moveDestination, skill, target, predictedDamage)
					});
				}
			}
		}

		if (candidates.Count == 0) return ChooseApproachOnlyAction(moveDestinations);
		candidates.Sort((left, right) => right.Score.CompareTo(left.Score));

		float selectedScore = candidates[0].Score;
		if (GD.Randf() < SuboptimalActionChance)
		{
			foreach (var candidate in candidates)
			{
				if (candidate.Score < selectedScore - ScoreComparisonTolerance)
				{
					selectedScore = candidate.Score;
					break;
				}
			}
		}

		var tiedCandidates = new List<ActionCandidate>();
		foreach (var candidate in candidates)
		{
			if (Mathf.Abs(candidate.Score - selectedScore) <= ScoreComparisonTolerance)
			{
				tiedCandidates.Add(candidate);
			}
		}

		int selectedIndex = (int)GD.RandRange(0, tiedCandidates.Count - 1);
		return tiedCandidates[selectedIndex];
	}

	private static bool DealsDamageToPlayer(Dictionary<Piece, int> predictedDamage)
	{
		foreach (var entry in predictedDamage)
		{
			if (entry.Value > 0 && entry.Key.Faction == Faction.Player) return true;
		}

		return false;
	}

	private float ScoreAction(Tile moveDestination, Skill skill, Tile target, Dictionary<Piece, int> predictedDamage)
	{
		// Sum total damage per side separately, then weight it: every point dealt to a
		// player is worth PlayerTargetScore, every point dealt to an ally costs FriendlyFireScore.
		// predictedDamage already includes knockback collision damage (both the knocked piece
		// and any piece it collides into), so friendly-fire collision is counted here too.
		int playerDamage = 0;
		int enemyDamage = 0;
		foreach (var entry in predictedDamage)
		{
			if (entry.Key.Faction == Faction.Enemy) enemyDamage += entry.Value;
			else playerDamage += entry.Value;
		}

		float score = playerDamage * PlayerTargetScore + enemyDamage * FriendlyFireScore;
		score += ScoreLocation(moveDestination);
		Vector2I targetOffset = GetRelativeTargetOffset(moveDestination, target);
		if (_hasPreviousAction && _lastPreparedSkill == skill && _lastPreparedTargetOffset == targetOffset)
		{
			score += RepeatActionPenalty;
		}
		return score;
	}

	private float ScoreLocation(Tile tile)
	{
		int nearestPlayerDistance = int.MaxValue;
		foreach (var candidateTile in WorldRef.Tiles.Values)
		{
			var occupant = candidateTile.OccupiedPiece;
			if (occupant == null || occupant.IsDead || occupant.Faction != Faction.Player) continue;
			nearestPlayerDistance = Math.Min(nearestPlayerDistance, Function.HexDistance(tile.GridPosition, candidateTile.GridPosition));
		}

		return nearestPlayerDistance == int.MaxValue ? 0f : -nearestPlayerDistance * ApproachWeight;
	}

	private ActionCandidate ChooseApproachOnlyAction(List<Tile> moveDestinations)
	{
		Tile bestTile = null;
		float bestScore = float.NegativeInfinity;
		foreach (var tile in moveDestinations)
		{
			float score = ScoreLocation(tile);
			if (score <= bestScore) continue;
			bestScore = score;
			bestTile = tile;
		}

		return bestTile == null ? null : new ActionCandidate { MoveDestination = bestTile, Score = bestScore };
	}

	private void PrepareAction(ActionCandidate action)
	{
		_preparedSkill?.ClearCombatPreview();
		_preparedSkill = action?.Skill;
		_preparedTarget = action?.Target;
		if (_preparedSkill == null || _preparedTarget == null) return;
		FaceTowards(_preparedTarget.GlobalPosition);

		_preparedTargetOffset = GetRelativeTargetOffset(OccupiedTile, _preparedTarget);
		_lastPreparedSkill = _preparedSkill;
		_lastPreparedTargetOffset = _preparedTargetOffset;
		_hasPreviousAction = true;
		_preparedSkill.ShowCombatPreview(_preparedTarget);
	}

	private bool RefreshPreparedTarget()
	{
		if (_preparedSkill == null || OccupiedTile == null || WorldRef == null) return false;

		var originAxial = Function.OffsetToAxial(OccupiedTile.GridPosition.X, OccupiedTile.GridPosition.Y);
		var targetPosition = Function.AxialToOffset(
			originAxial.X + _preparedTargetOffset.X,
			originAxial.Y + _preparedTargetOffset.Y
		);
		var target = WorldRef.GetTile(targetPosition);
		if (target == null || !_preparedSkill.CanTargetFrom(OccupiedTile, target))
		{
			CancelPreparedAttack();
			return false;
		}

		_preparedTarget = target;
		return true;
	}

	private void CancelPreparedAttack()
	{
		_preparedSkill?.ClearCombatPreview();
		_preparedSkill = null;
		_preparedTarget = null;
		_preparedTargetOffset = Vector2I.Zero;
	}

	private static Vector2I GetRelativeTargetOffset(Tile origin, Tile target)
	{
		var originAxial = Function.OffsetToAxial(origin.GridPosition.X, origin.GridPosition.Y);
		var targetAxial = Function.OffsetToAxial(target.GridPosition.X, target.GridPosition.Y);
		return targetAxial - originAxial;
	}
}
