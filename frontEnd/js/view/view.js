// view.js
export default class MatrixView {
    constructor() {

        this.quadrantes = document.querySelectorAll('.quadrant'); 
    }

    createTaskDOM(tarefa) {
        const li = document.createElement('li');
        li.id = tarefa.id; 

        const span = document.createElement('span');
        span.textContent = tarefa.texto;
        span.classList.add('task-text');

        const deleteBtn = document.createElement('button');
        deleteBtn.textContent = 'X';
        deleteBtn.classList.add('delete-btn');

        li.appendChild(span);
        li.appendChild(deleteBtn);

        return li;
    }

    displayTasks(tarefas) {
        this.quadrantes.forEach(q => {
            q.querySelector('.task-list').innerHTML = '';
        });

        tarefas.forEach(tarefa => {
            const li = this.createTaskDOM(tarefa);
            
            const list = document.querySelector(`.${tarefa.quadrante} .task-list`);
            if (list) {
                list.appendChild(li);
            }
        });
    }

    bindEditTask(handler) {
        const matrixContainer = document.querySelector('.eisenhower-matrix');

        matrixContainer.addEventListener('dblclick', event => {
            if (event.target.classList.contains('task-text')) {
                const span = event.target;
                span.setAttribute('contenteditable', true); 
                span.focus(); 
            }
        });

        matrixContainer.addEventListener('focusout', event => {
            if (event.target.classList.contains('task-text')) {
                const span = event.target;
                span.removeAttribute('contenteditable'); 
                
                const id = parseInt(span.parentElement.id); 
                const novoTexto = span.textContent.trim();
                
                if (novoTexto) {
                    handler(id, novoTexto); 
                }
            }
        });

        matrixContainer.addEventListener('keydown', event => {
            if (event.target.classList.contains('task-text')) {
                if (event.key === 'Enter') {
                    event.preventDefault(); 
                    event.target.blur(); 
                }
            }
        });
    }
}