import 'emoji-picker-element';
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, OnInit, signal, ViewChild, CUSTOM_ELEMENTS_SCHEMA, HostListener, inject, effect } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { TodoService } from '../../../core/services/todo.service';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TodoItem, ReorderItem } from '../../../core/models/todo.model';
import { TodoCardComponent } from '../todo-card/todo-card.component';

@Component({
  selector: 'app-todo-board',
  standalone: true,
  imports: [FormsModule, DragDropModule, TodoCardComponent, LucideAngularModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './todo-board.component.html'
})
export class TodoBoardComponent implements OnInit {
  private todoService = inject(TodoService);
  private authService = inject(AuthService);
  private destroyRef = inject(DestroyRef);
  themeService = inject(ThemeService);

  @ViewChild('addInput') addInput!: ElementRef<HTMLInputElement>;

  activeTodos = signal<TodoItem[]>([]);
  completedTodos = signal<TodoItem[]>([]);
  newTitle = signal('');
  newEmoji = signal<string | null>(null);
  username = signal('');
  hoveredList = signal<'active' | 'completed' | null>(null);
  dragSource = signal<'active' | 'completed' | null>(null);
  showEmojiPicker = signal(false);
  fadingInId = signal<number | null>(null);
  collapsingId = signal<number | null>(null);

  constructor() {
    effect(() => {
      const hovered = this.hoveredList();
      document.body.classList.remove('dragging-to-active', 'dragging-to-completed');
      if (hovered) document.body.classList.add(`dragging-to-${hovered}`);
    });
  }

  ngOnInit(): void {
    this.username.set(this.authService.getUsername() ?? '');
    this.loadTodos();
  }

  loadTodos(): void {
    this.todoService.getAll().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: todos => {
        this.activeTodos.set(todos.filter(t => !t.isCompleted).sort((a, b) => a.sortOrder - b.sortOrder));
        this.completedTodos.set(todos.filter(t => t.isCompleted).sort((a, b) => a.sortOrder - b.sortOrder));
      },
      error: err => console.error('Failed to load todos', err)
    });
  }

  addTodo(): void {
    const title = this.newTitle().trim();
    if (!title) return;
    this.todoService.create(title, this.newEmoji()).subscribe({
      next: todo => {
        this.activeTodos.update(list => [...list, todo]);
        this.triggerFadeIn(todo.id);
        this.newTitle.set('');
        this.newEmoji.set(null);
        this.addInput.nativeElement.focus();
      },
      error: err => console.error('Failed to create todo', err)
    });
  }

  toggleEmojiPicker(): void {
    this.showEmojiPicker.update(v => !v);
  }

  onEmojiSelect(event: Event): void {
    const emoji = (event as CustomEvent).detail.unicode;
    this.newEmoji.set(emoji);
    this.showEmojiPicker.set(false);
    this.addInput.nativeElement.focus();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.showEmojiPicker()) return;
    const target = event.target as HTMLElement;
    if (!target.closest('.emoji-wrapper')) {
      this.showEmojiPicker.set(false);
    }
  }

  onDragStarted(source: 'active' | 'completed'): void {
    this.dragSource.set(source);
  }

  onDragEnded(): void {
    this.dragSource.set(null);
    this.hoveredList.set(null);
  }

  drop(event: CdkDragDrop<TodoItem[]>): void {
    this.hoveredList.set(null);
    this.dragSource.set(null);
    const isCompletedTarget = event.container.id === 'completed-list';
    const active = [...this.activeTodos()];
    const completed = [...this.completedTodos()];
    const sourceList = event.previousContainer.id === 'active-list' ? active : completed;
    const targetList = event.container.id === 'active-list' ? active : completed;

    if (event.previousContainer === event.container) {
      moveItemInArray(targetList, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(sourceList, targetList, event.previousIndex, event.currentIndex);
      targetList[event.currentIndex] = { ...targetList[event.currentIndex], isCompleted: isCompletedTarget };
    }

    this.activeTodos.set(active);
    this.completedTodos.set(completed);

    const reorderItems: ReorderItem[] = targetList.map((todo, i) => ({
      id: todo.id,
      sortOrder: i,
      isCompleted: isCompletedTarget
    }));

    if (event.previousContainer !== event.container) {
      const sourceIsCompleted = event.container.id !== 'completed-list';
      const sourceItems: ReorderItem[] = sourceList.map((todo, i) => ({
        id: todo.id,
        sortOrder: i,
        isCompleted: sourceIsCompleted
      }));
      this.todoService.reorder([...reorderItems, ...sourceItems]).subscribe({
        error: err => console.error('Failed to reorder', err)
      });
    } else {
      this.todoService.reorder(reorderItems).subscribe({
        error: err => console.error('Failed to reorder', err)
      });
    }
  }

  toggleTodo(id: number, which: 'active' | 'completed'): void {
    if (this.collapsingId() !== null) return;
    this.collapsingId.set(id);
    setTimeout(() => this.performToggle(id, which), 270);
  }

  private performToggle(id: number, which: 'active' | 'completed'): void {
    this.collapsingId.set(null);
    const movingToCompleted = which === 'active';
    const source = movingToCompleted ? [...this.activeTodos()] : [...this.completedTodos()];
    const target = movingToCompleted ? [...this.completedTodos()] : [...this.activeTodos()];

    const idx = source.findIndex(t => t.id === id);
    if (idx === -1) return;

    const [todo] = source.splice(idx, 1);
    const moved = { ...todo, isCompleted: movingToCompleted };
    target.push(moved);
    this.triggerFadeIn(moved.id);

    this.activeTodos.set(movingToCompleted ? source : target);
    this.completedTodos.set(movingToCompleted ? target : source);

    const reorderItems: ReorderItem[] = [
      ...source.map((t, i) => ({ id: t.id, sortOrder: i, isCompleted: !movingToCompleted })),
      ...target.map((t, i) => ({ id: t.id, sortOrder: i, isCompleted: movingToCompleted }))
    ];
    this.todoService.reorder(reorderItems).subscribe({
      error: err => console.error('Failed to toggle todo', err)
    });
  }

  private triggerFadeIn(id: number): void {
    this.fadingInId.set(id);
    setTimeout(() => this.fadingInId.set(null), 500);
  }

  deleteTodo(id: number, which: 'active' | 'completed'): void {
    this.todoService.delete(id).subscribe({
      next: () => {
        if (which === 'active') {
          this.activeTodos.update(list => list.filter(t => t.id !== id));
        } else {
          this.completedTodos.update(list => list.filter(t => t.id !== id));
        }
      },
      error: err => console.error('Failed to delete todo', err)
    });
  }

  renameTodo(id: number, title: string, which: 'active' | 'completed'): void {
    this.todoService.update(id, { title }).subscribe({
      next: updated => {
        const patch = (list: TodoItem[]) =>
          list.map(t => t.id === id ? { ...t, title: updated.title } : t);
        if (which === 'active') {
          this.activeTodos.update(patch);
        } else {
          this.completedTodos.update(patch);
        }
      },
      error: err => console.error('Failed to rename todo', err)
    });
  }

  changeEmoji(id: number, emoji: string | null, which: 'active' | 'completed'): void {
    this.todoService.update(id, { emoji: emoji ?? '' }).subscribe({
      next: updated => {
        const patch = (list: TodoItem[]) =>
          list.map(t => t.id === id ? { ...t, emoji: updated.emoji } : t);
        if (which === 'active') {
          this.activeTodos.update(patch);
        } else {
          this.completedTodos.update(patch);
        }
      },
      error: err => console.error('Failed to update emoji', err)
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
