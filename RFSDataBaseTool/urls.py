from django.contrib import admin
from django.urls import path
from . import views

urlpatterns = [
    path('', views.home, name='home'),
    path('/Tool', views.db_list, name='db_list'),
    path('/Raw', views.raw_db_list, name='raw_db_list'),
]
