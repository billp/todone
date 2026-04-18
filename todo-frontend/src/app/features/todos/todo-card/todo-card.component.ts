import 'emoji-picker-element';
import { ChangeDetectionStrategy, Component, ElementRef, HostListener, CUSTOM_ELEMENTS_SCHEMA, ViewChild, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { TodoItem } from '../../../core/models/todo.model';

@Component({
  selector: 'app-todo-card',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './todo-card.component.html'
})
export class TodoCardComponent {
  todo = input.required<TodoItem>();

  deleted = output<void>();
  renamed = output<string>();
  emojiChanged = output<string | null>();
  toggled = output<void>();

  @ViewChild('editInput') editInput!: ElementRef<HTMLInputElement>;

  editing = signal(false);
  editTitle = '';
  showEmojiPicker = signal(false);
  pickerOpensDown = signal(false);

  startEdit(): void {
    this.editTitle = this.todo().title;
    this.editing.set(true);
  }

  saveEdit(): void {
    const trimmed = this.editTitle.trim();
    if (trimmed && trimmed !== this.todo().title) {
      this.renamed.emit(trimmed);
    }
    this.editing.set(false);
  }

  cancelEdit(): void {
    this.editing.set(false);
  }

  toggleEmojiPicker(event: MouseEvent): void {
    event.stopPropagation();
    if ((event.target as HTMLElement).closest('emoji-picker')) return;
    if (!this.showEmojiPicker()) {
      const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
      this.pickerOpensDown.set(rect.top < 460);
    }
    this.showEmojiPicker.update(v => !v);
  }

  onEmojiSelect(event: Event): void {
    const emoji = (event as CustomEvent).detail.unicode;
    this.emojiChanged.emit(emoji);
    this.showEmojiPicker.set(false);
  }

  clearEmoji(event: MouseEvent): void {
    event.stopPropagation();
    this.emojiChanged.emit(null);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.showEmojiPicker()) return;
    const target = event.target as HTMLElement;
    if (!target.closest('.todo-emoji-btn')) {
      this.showEmojiPicker.set(false);
    }
  }
}
