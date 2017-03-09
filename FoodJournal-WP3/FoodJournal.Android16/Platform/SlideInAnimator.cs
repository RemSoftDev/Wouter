using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Support.V4.View;
using System.Collections.Generic;
using Java.Lang;

namespace FoodJournal.Android15
{
	#if false 
	public class SlideInItemAnimator : RecyclerView.ItemAnimator {

		public RecyclerView mRecyclerView;

		private List<RecyclerView.ViewHolder> mPendingRemovals = new List<RecyclerView.ViewHolder>();
		private List<RecyclerView.ViewHolder> mPendingAdditions = new List<RecyclerView.ViewHolder>();
		private List<MoveInfo> mPendingMoves = new List<MoveInfo>();

		private List<RecyclerView.ViewHolder> mAdditions = new List<RecyclerView.ViewHolder>();
		private List<MoveInfo> mMoves = new List<MoveInfo>();

		private List<RecyclerView.ViewHolder> mAddAnimations = new List<RecyclerView.ViewHolder>();
		private List<RecyclerView.ViewHolder> mMoveAnimations = new List<RecyclerView.ViewHolder>();
		private List<RecyclerView.ViewHolder> mRemoveAnimations = new List<RecyclerView.ViewHolder>();


		public SlideInItemAnimator(RecyclerView recyclerView){
			mRecyclerView = recyclerView;
		}

		public class VpaListenerAdapter : IViewPropertyAnimatorListener {

			public void onAnimationStart(View view) {}

			public void onAnimationEnd(View view) {}

			public void onAnimationCancel(View view) {}
		};

		private void animateRemoveImpl(RecyclerView.ViewHolder holder) {
			View view = holder.ItemView;
			ViewCompat.Animate(view).Cancel();
			ViewCompat.Animate(view).SetDuration(RemoveDuration).
			TranslationX(-mRecyclerView.Width).WithEndAction(() =>
				{
					ViewCompat.SetTranslationX(view, -mRecyclerView.getWidth());
					dispatchRemoveFinished(holder);
					mRemoveAnimations.remove(holder);
					dispatchFinishedWhenDone();
				}
			).Start();
			mRemoveAnimations.Add(holder);
		}

		public override bool AnimateAdd (RecyclerView.ViewHolder holder)
		{
			ViewCompat.SetTranslationX(holder.itemView, -mRecyclerView.getWidth());
			mPendingAdditions.add(holder);
			return true;
		}

		private void animateAddImpl(RecyclerView.ViewHolder holder) {
			View view = holder.ItemView;

			ViewCompat.Animate(view).Cancel();
			ViewCompat.Animate(view).TranslationX(0)
				.SetDuration(AddDuration).WithEndAction(() => {
					
//				SetListener(new VpaListenerAdapter() {
//					@Override
//					public void onAnimationCancel(View view) {
//						ViewCompat.setTranslationX(view, 0);
//					}
//
//					@Override
//					public void onAnimationEnd(View view) {
						dispatchAddFinished(holder);
						mAddAnimations.remove(holder);
						dispatchFinishedWhenDone();
					}
				).Start();
			mAddAnimations.add(holder);
		}


		private class MoveInfo {
			public RecyclerView.ViewHolder holder;
			public int fromX, fromY, toX, toY;

			private MoveInfo(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY) {
				this.holder = holder;
				this.fromX = fromX;
				this.fromY = fromY;
				this.toX = toX;
				this.toY = toY;
			}
		}


		public override void RunPendingAnimations ()
		{
			bool removalsPending = !mPendingRemovals.isEmpty();
			bool movesPending = !mPendingMoves.isEmpty();
			bool additionsPending = !mPendingAdditions.isEmpty();
			if (!removalsPending && !movesPending && !additionsPending) {
				// nothing to animate
				return;
			}
			// First, remove stuff
			foreach (RecyclerView.ViewHolder holder in mPendingRemovals) {
				animateRemoveImpl(holder);
			}
			mPendingRemovals.clear();
			// Next, move stuff
			if (movesPending) {
				mMoves.addAll(mPendingMoves);
				mPendingMoves.clear();
				Runnable mover = new Runnable ();
//				mover. {
//					@Override
//					public void run() {
//						for (MoveInfo moveInfo : mMoves) {
//							animateMoveImpl(moveInfo.holder, moveInfo.fromX, moveInfo.fromY,
//								moveInfo.toX, moveInfo.toY);
//						}
//						mMoves.clear();
//					}
//				};
				if (removalsPending) {
					View view = mMoves.get(0).holder.itemView;
					ViewCompat.postOnAnimationDelayed(view, mover, getRemoveDuration());
				} else {
					mover.run();
				}
			}
			// Next, add stuff
			if (additionsPending) {
				mAdditions.addAll(mPendingAdditions);
				mPendingAdditions.clear();
				Runnable adder = new Runnable ();
//				{
//					public void run() {
//						for (RecyclerView.ViewHolder holder : mAdditions) {
//							animateAddImpl(holder);
//						}
//						mAdditions.clear();
//					}
//				};
				if (removalsPending || movesPending) {
					View view = mAdditions.get(0).itemView;
					ViewCompat.postOnAnimationDelayed(view, adder,
						(removalsPending ? getRemoveDuration() : 0) +
						(movesPending ? getMoveDuration() : 0));
				} else {
					adder.run();
				}
			}
		}

		public override bool AnimateRemove (RecyclerView.ViewHolder holder)
		{
			mPendingRemovals.add(holder);
			return true;
		}

		public override bool AnimateMove (RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
		{
			int toX, int toY;
			View view = holder.itemView;
			int deltaX = toX - fromX;
			int deltaY = toY - fromY;
			if (deltaX == 0 && deltaY == 0) {
				dispatchMoveFinished(holder);
				return false;
			}
			if (deltaX != 0) {
				ViewCompat.setTranslationX(view, -deltaX);
			}
			if (deltaY != 0) {
				ViewCompat.setTranslationY(view, -deltaY);
			}
			mPendingMoves.add(new MoveInfo(holder, fromX, fromY, toX, toY));
			return true;
		}

		public override bool AnimateChange (RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromX, int fromY, int toX, int toY)
		{
			return false;
		}

		private void animateMoveImpl(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY) {
			final View view = holder.itemView;
			final int deltaX = toX - fromX;
			final int deltaY = toY - fromY;
			ViewCompat.Animate(view).cancel();
			if (deltaX != 0) {
				ViewCompat.Animate(view).translationX(0);
			}
			if (deltaY != 0) {
				ViewCompat.Animate(view).translationY(0);
			}
			// TODO: make EndActions end listeners instead, since end actions aren't called when
			// vpas are canceled (and can't end them. why?)
			// need listener functionality in VPACompat for this. Ick.
				ViewCompat.Animate(view).SetDuration(MoveDuration).WithEndAction(()={
					
//					.setListener(new VpaListenerAdapter() {
//				@Override
//				public void onAnimationCancel(View view) {
//					if (deltaX != 0) {
//						ViewCompat.setTranslationX(view, 0);
//					}
//					if (deltaY != 0) {
//						ViewCompat.setTranslationY(view, 0);
//					}
//				}
//				@Override
//				public void onAnimationEnd(View view) {
					dispatchMoveFinished(holder);
					mMoveAnimations.remove(holder);
					dispatchFinishedWhenDone();
				}
			).Start();
			mMoveAnimations.add(holder);
		}

		public override void EndAnimation (RecyclerView.ViewHolder item)
		{
			View view = item.itemView;
			ViewCompat.Animate(view).Cancel();
			if (mPendingMoves.Contains(item)) {
				ViewCompat.SetTranslationY(view, 0);
				ViewCompat.SetTranslationX(view, 0);
				dispatchMoveFinished(item);
				mPendingMoves.remove(item);
			}
			if (mPendingRemovals.contains(item)) {
				dispatchRemoveFinished(item);
				mPendingRemovals.remove(item);
			}
			if (mPendingAdditions.contains(item)) {
				ViewCompat.setAlpha(view, 1);
				dispatchAddFinished(item);
				mPendingAdditions.remove(item);
			}
			if (mMoveAnimations.contains(item)) {
				ViewCompat.setTranslationY(view, 0);
				ViewCompat.setTranslationX(view, 0);
				dispatchMoveFinished(item);
				mMoveAnimations.remove(item);
			}
			if (mRemoveAnimations.contains(item)) {
				ViewCompat.setAlpha(view, 1);
				dispatchRemoveFinished(item);
				mRemoveAnimations.remove(item);
			}
			if (mAddAnimations.contains(item)) {
				ViewCompat.setAlpha(view, 1);
				dispatchAddFinished(item);
				mAddAnimations.remove(item);
			}
			dispatchFinishedWhenDone();
		}

		public override bool IsRunning {
			get {
				return (!mMoveAnimations.isEmpty() ||
					!mRemoveAnimations.isEmpty() ||
					!mAddAnimations.isEmpty() ||
					!mMoves.isEmpty() ||
					!mAdditions.isEmpty());
			}
		}

		/**
     * Check the state of currently pending and running animations. If there are none
     * pending/running, call {@link #dispatchAnimationsFinished()} to notify any
     * listeners.
     */
		private void dispatchFinishedWhenDone() {
			if (!isRunning()) {
				dispatchAnimationsFinished();
			}
		}

		public override void EndAnimations ()
		{
			int count = mPendingMoves.size();
			for (int i = count - 1; i >= 0; i--) {
				MoveInfo item = mPendingMoves.get(i);
				View view = item.holder.itemView;
				ViewCompat.animate(view).cancel();
				ViewCompat.setTranslationY(view, 0);
				ViewCompat.setTranslationX(view, 0);
				dispatchMoveFinished(item.holder);
				mPendingMoves.remove(item);
			}
			count = mPendingRemovals.size();
			for (int i = count - 1; i >= 0; i--) {
				RecyclerView.ViewHolder item = mPendingRemovals.get(i);
				dispatchRemoveFinished(item);
				mPendingRemovals.remove(item);
			}
			count = mPendingAdditions.size();
			for (int i = count - 1; i >= 0; i--) {
				RecyclerView.ViewHolder item = mPendingAdditions.get(i);
				View view = item.itemView;
				ViewCompat.setAlpha(view, 1);
				dispatchAddFinished(item);
				mPendingAdditions.remove(item);
			}
			if (!isRunning()) {
				return;
			}
			count = mMoveAnimations.size();
			for (int i = count - 1; i >= 0; i--) {
				RecyclerView.ViewHolder item = mMoveAnimations.get(i);
				View view = item.itemView;
				ViewCompat.animate(view).cancel();
				ViewCompat.setTranslationY(view, 0);
				ViewCompat.setTranslationX(view, 0);
				dispatchMoveFinished(item);
				mMoveAnimations.remove(item);
			}
			count = mRemoveAnimations.size();
			for (int i = count - 1; i >= 0; i--) {
				RecyclerView.ViewHolder item = mRemoveAnimations.get(i);
				View view = item.itemView;
				ViewCompat.animate(view).cancel();
				ViewCompat.setAlpha(view, 1);
				dispatchRemoveFinished(item);
				mRemoveAnimations.remove(item);
			}
			count = mAddAnimations.size();
			for (int i = count - 1; i >= 0; i--) {
				RecyclerView.ViewHolder item = mAddAnimations.get(i);
				View view = item.itemView;
				ViewCompat.animate(view).cancel();
				ViewCompat.setAlpha(view, 1);
				dispatchAddFinished(item);
				mAddAnimations.remove(item);
			}
			mMoves.clear();
			mAdditions.clear();
			dispatchAnimationsFinished();
		}


	}

#endif
}

