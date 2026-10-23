from django.shortcuts import render
from django.utils import timezone
from .models import Post

# Create your views here.
def home(request):
    return render(request, 'redflagstudio/home.html', {})
def db_list(request):
    posts = Post.objects.filter(published_date__lte=timezone.now()).order_by('published_date')
    return render(request, 'redflagstudio/db_list.html', {})
def raw_db_list(request):
    posts = Post.objects.filter(published_date__lte=timezone.now()).order_by('published_date')
    return render(request, 'redflagstudio/raw_db_list.html', {})